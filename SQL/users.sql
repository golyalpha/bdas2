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
        -- Validace: nelze být sám sobě náhradníkem
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

END user_management_pkg;



