CREATE OR REPLACE PACKAGE countries_pkg AS
    PROCEDURE persist_country(
        p_name IN VARCHAR2
    );
    
    -- Smazané země (CASCADE smaže i závislá města)
    PROCEDURE delete_country(
        p_id_country IN NUMBER
    );
END countries_pkg;
/

CREATE OR REPLACE PACKAGE BODY countries_pkg AS

    PROCEDURE persist_country(
        p_name IN VARCHAR2
    ) IS
        v_count NUMBER;
    BEGIN
        DBMS_OUTPUT.PUT_LINE('=== PERSIST_COUNTRY START ===');
        DBMS_OUTPUT.PUT_LINE('Název: ' || p_name);
        
        -- Vložení nové země
        INSERT INTO countries (
            id_country,
            "name"
        ) VALUES (
            countries_id_country_seq.NEXTVAL,
            TRIM(p_name)
        );
        
        DBMS_OUTPUT.PUT_LINE('Země úspěšně vytvořena');
        COMMIT;
        
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('CHYBA: ' || SQLERRM);
            ROLLBACK;
            RAISE;
    END persist_country;


    PROCEDURE delete_country(
        p_id_country IN NUMBER
    ) IS
        v_country_name countries."name"%TYPE;
        v_city_count NUMBER;
    BEGIN
        DBMS_OUTPUT.PUT_LINE('=== DELETE_COUNTRY START ===');
        DBMS_OUTPUT.PUT_LINE('ID země: ' || p_id_country);
        
        -- Kontrola existence země
        BEGIN
            SELECT "name" INTO v_country_name
            FROM countries
            WHERE id_country = p_id_country;
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                RAISE_APPLICATION_ERROR(-20404, 'Země s ID ' || p_id_country || ' neexistuje');
        END;
        
        DBMS_OUTPUT.PUT_LINE('Název země: ' || v_country_name);
        
        -- Smazání země (CASCADE smaže i města)
        DELETE FROM countries
        WHERE id_country = p_id_country;

        COMMIT;
        
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('CHYBA: ' || SQLERRM);
            ROLLBACK;
            RAISE;
    END delete_country;

END countries_pkg;
/