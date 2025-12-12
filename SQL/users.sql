CREATE OR REPLACE PACKAGE user_management_pkg AS

    PROCEDURE persist_organizer(
        p_id_organizer IN NUMBER DEFAULT NULL,
        p_name IN VARCHAR2,
        p_email IN VARCHAR2,
        p_id_role IN NUMBER DEFAULT NULL,
        p_id_organizer_substitute IN NUMBER DEFAULT NULL
    );

    PROCEDURE persist_credential(
        p_id_credential IN NUMBER DEFAULT NULL,
        p_credential_type IN CHAR,
        p_data IN VARCHAR2,
        p_id_organizer IN NUMBER
    );

    PROCEDURE get_organizers_hierarchy(
        p_cursor OUT SYS_REFCURSOR
    );

    PROCEDURE set_organizer_substitute(
        p_id_organizer IN NUMBER,
        p_id_organizer_substitute IN NUMBER DEFAULT NULL
    );

    PROCEDURE get_non_admin_organizers(
        p_cursor OUT SYS_REFCURSOR
    );

    FUNCTION hash_password(
        p_password IN VARCHAR2
    ) RETURN VARCHAR2;

    PROCEDURE register_user(
        p_name IN VARCHAR2,
        p_email IN VARCHAR2,
        p_password IN VARCHAR2,
        p_id_role IN NUMBER DEFAULT 3,  -- Default = Guest
        o_organizer_id OUT NUMBER
    );

    FUNCTION verify_password(
        p_email IN VARCHAR2,
        p_password IN VARCHAR2
    ) RETURN NUMBER;
    
    -- Reset hesla bez ověření (pro zapomenuté heslo)
    PROCEDURE reset_password(
        p_email IN VARCHAR2,
        p_new_password IN VARCHAR2
    );
    
END user_management_pkg;
/


CREATE OR REPLACE PACKAGE BODY user_management_pkg AS

    PROCEDURE persist_organizer (
        p_id_organizer IN NUMBER DEFAULT NULL,
        p_name IN VARCHAR2,
        p_email IN VARCHAR2,
        p_id_role IN NUMBER DEFAULT NULL,
        p_id_organizer_substitute IN NUMBER DEFAULT NULL    
    ) IS
        v_new_id NUMBER;
    BEGIN
        IF p_id_organizer IS NULL OR p_id_organizer = 0 THEN
            SELECT organizers_id_organizer_seq.NEXTVAL INTO v_new_id FROM dual;

            INSERT INTO organizers (id_organizer, "name", email, id_role, id_organizer_substitute)
            VALUES (v_new_id, p_name, p_email, p_id_role, p_id_organizer_substitute);
        ELSE
            UPDATE organizers
            SET "name" = p_name,
                email = p_email,
                id_role = p_id_role
            WHERE id_organizer = p_id_organizer;

            IF SQL%ROWCOUNT = 0 THEN
                RAISE_APPLICATION_ERROR(-20001, 'No organizer found with the given ID.');
            END IF;
        END IF;

        COMMIT;
    END persist_organizer;

    PROCEDURE persist_credential(
        p_id_credential IN NUMBER DEFAULT NULL,
        p_credential_type IN CHAR,
        p_data IN VARCHAR2,
        p_id_organizer IN NUMBER
    ) IS
        v_new_id NUMBER;
    BEGIN
        IF p_id_credential IS NULL THEN
            SELECT credentials_id_credential_seq.NEXTVAL INTO v_new_id FROM dual;

            INSERT INTO credentials (id_credential, credential_type, "data", created_at, id_organizer)
            VALUES (v_new_id, p_credential_type, p_data, CURRENT_TIMESTAMP, p_id_organizer);
        ELSE
            RAISE_APPLICATION_ERROR(-20001, 'Credentials are not mutable.');
        END IF;

        COMMIT;
    END persist_credential;

    PROCEDURE get_organizers_hierarchy(
        p_cursor OUT SYS_REFCURSOR
    )
    IS
    BEGIN
        OPEN p_cursor FOR
        SELECT 
            LEVEL as hierarchy_level,
            id_organizer,
            "name",
            email,
            id_organizer_substitute,
            SUBSTR(SYS_CONNECT_BY_PATH("name", ' → '), 4) as hierarchy_path,
            CONNECT_BY_ROOT "name" as top_organizer
        FROM organizers
        START WITH id_organizer_substitute IS NULL
        CONNECT BY PRIOR id_organizer = id_organizer_substitute
        ORDER SIBLINGS BY "name";
    END get_organizers_hierarchy;

    PROCEDURE set_organizer_substitute(
        p_id_organizer IN NUMBER,
        p_id_organizer_substitute IN NUMBER DEFAULT NULL
    ) IS
    BEGIN
        IF p_id_organizer = p_id_organizer_substitute THEN
            RAISE_APPLICATION_ERROR(-20010, 'Uživatel nemůže být sám sobě náhradníkem.');
        END IF;
        
        -- Dočasně vypnout trigger
        EXECUTE IMMEDIATE 'ALTER TRIGGER fkntm_organizers DISABLE';
        
        UPDATE organizers
        SET id_organizer_substitute = p_id_organizer_substitute
        WHERE id_organizer = p_id_organizer;
        
        IF SQL%ROWCOUNT = 0 THEN
            EXECUTE IMMEDIATE 'ALTER TRIGGER fkntm_organizers ENABLE';
            RAISE_APPLICATION_ERROR(-20001, 'Organizátor s daným ID nebyl nalezen.');
        END IF;
        
        -- Znovu zapnout trigger
        EXECUTE IMMEDIATE 'ALTER TRIGGER fkntm_organizers ENABLE';
        
        COMMIT;
    END set_organizer_substitute;

    PROCEDURE get_non_admin_organizers(
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                o.id_organizer,
                o."name",
                o.email,
                o.id_role,
                r."name" as role_name
            FROM organizers o
            JOIN roles r ON o.id_role = r.id_role
            WHERE r."name" != 'Administrator'
            ORDER BY o."name";
    END get_non_admin_organizers;

    
    FUNCTION hash_password(
        p_password VARCHAR2
    ) RETURN VARCHAR2
    IS
        v_password_raw RAW(2000);
        v_hash RAW(2000);
    BEGIN
        -- Konverze stringu na RAW
        v_password_raw := UTL_RAW.CAST_TO_RAW(p_password);
        
        -- MD5 hash pomocí DBMS_OBFUSCATION_TOOLKIT
        DBMS_OBFUSCATION_TOOLKIT.MD5(
            input => v_password_raw,
            checksum => v_hash
        );
        
        -- Vrátí hash jako HEX string (32 znaků pro MD5)
        RETURN RAWTOHEX(v_hash);
    EXCEPTION
        WHEN OTHERS THEN
            RAISE_APPLICATION_ERROR(-20001, 'Chyba při hashování hesla: ' || SQLERRM);
    END hash_password;
    

    PROCEDURE register_user(
        p_name VARCHAR2,
        p_email VARCHAR2,
        p_password VARCHAR2,
        p_id_role NUMBER DEFAULT 3,  -- Default = Guest
        o_organizer_id OUT NUMBER
    )
    IS
        v_hashed_password VARCHAR2(256);
        v_email_exists NUMBER;
    BEGIN
        -- 1. Kontrola existence emailu
        SELECT COUNT(*) INTO v_email_exists
        FROM organizers
        WHERE email = p_email;
        
        IF v_email_exists > 0 THEN
            RAISE_APPLICATION_ERROR(-20100, 'Email již existuje');
        END IF;
        
        -- 2. Vložení organizátora
        INSERT INTO organizers ("name", email, id_role)
        VALUES (p_name, p_email, p_id_role)
        RETURNING id_organizer INTO o_organizer_id;
        
        -- 3. Hashování hesla pomocí funkce
        v_hashed_password := hash_password(p_password);
        
        -- 4. Uložení credentials
        INSERT INTO credentials (credential_type, "data", created_at, id_organizer)
        VALUES ('PASSWORD', v_hashed_password, SYSTIMESTAMP, o_organizer_id);
        
        COMMIT;
        
        DBMS_OUTPUT.PUT_LINE('Uživatel ' || p_name || ' byl úspěšně zaregistrován s ID: ' || o_organizer_id);
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            RAISE;
    END register_user;

    FUNCTION verify_password(
        p_email VARCHAR2,
        p_password VARCHAR2
    ) RETURN NUMBER
    IS
        v_stored_hash VARCHAR2(256);
        v_computed_hash VARCHAR2(256);
        v_organizer_id NUMBER;
        v_credential_id NUMBER;
    BEGIN
        -- Načtení nejnovějšího hesla (ORDER BY created_at DESC + ROWNUM = 1)
        SELECT c."data", o.id_organizer, c.id_credential
        INTO v_stored_hash, v_organizer_id, v_credential_id
        FROM credentials c
        JOIN organizers o ON c.id_organizer = o.id_organizer
        WHERE o.email = p_email
        AND c.credential_type = 'PASSWORD';
        
        -- Výpočet hashe ze zadaného hesla
        v_computed_hash := hash_password(p_password);
        
        -- Porovnání hashů
        IF v_stored_hash = v_computed_hash THEN
            RETURN v_organizer_id;  -- Úspěch - vrátí ID uživatele
        ELSE
            RETURN NULL;  -- Neúspěch - neplatné heslo
        END IF;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RETURN NULL;  -- Uživatel neexistuje
        WHEN OTHERS THEN
            RAISE;
    END verify_password;

    PROCEDURE reset_password(
        p_email IN VARCHAR2,
        p_new_password IN VARCHAR2
    )
    IS
        v_new_hash VARCHAR2(256);
        v_credential_id NUMBER;
    BEGIN
        -- Najít uživatele podle emailu
        BEGIN
            SELECT c.id_credential
            INTO v_credential_id
            FROM organizers o
            JOIN credentials c ON o.id_organizer = c.id_organizer
            WHERE o.email = p_email
            AND c.credential_type = 'PASSWORD';
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                RAISE_APPLICATION_ERROR(-20104, 'Uživatel s tímto emailem nebyl nalezen');
        END;
        
        -- Zahashovat nové heslo
        v_new_hash := hash_password(p_new_password);
        
        --OPRAVA: AKTUALIZACE hesla místo INSERT
        UPDATE credentials
        SET "data" = v_new_hash,
            created_at = SYSTIMESTAMP
        WHERE id_credential = v_credential_id;
        
        COMMIT;
        
        DBMS_OUTPUT.PUT_LINE('Heslo pro email ' || p_email || ' bylo úspěšně resetováno');
    EXCEPTION
        WHEN OTHERS THEN
            ROLLBACK;
            RAISE;
    END reset_password;

END user_management_pkg;





