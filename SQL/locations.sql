CREATE OR REPLACE PACKAGE locations_pkg AS

        PROCEDURE persist_location(
        p_id_location IN NUMBER DEFAULT NULL,
        p_name IN VARCHAR2,
        p_start IN DATE,
        p_end IN DATE,
        p_id_city IN NUMBER,
        p_id_organizer IN NUMBER
    );

    PROCEDURE delete_location (
        p_id_location IN NUMBER
    );

END locations_pkg;
/

CREATE OR REPLACE PACKAGE BODY locations_pkg AS

    -- Edit or Create Location
    PROCEDURE persist_location(
        p_id_location IN NUMBER DEFAULT NULL,
        p_name IN VARCHAR2,
        p_start IN DATE,
        p_end IN DATE,
        p_id_city IN NUMBER,
        p_id_organizer IN NUMBER
    ) IS
        v_new_id NUMBER; -- For new location ID
    BEGIN
        IF p_id_location IS NULL OR p_id_location = 0 THEN
            -- Generate new ID using sequence
            SELECT locations_id_location_seq.NEXTVAL INTO v_new_id FROM dual;

            -- Insert new location
            INSERT INTO locations (
                id_location, "name", availability_start, availability_end, id_city, id_organizer
            ) VALUES (
                v_new_id, p_name, p_start, p_end, p_id_city, p_id_organizer
            );

        ELSE
            -- Update existing location
            UPDATE locations
            SET 
                "name" = p_name,
                availability_start = p_start,
                availability_end = p_end,
                id_city = p_id_city,
                id_organizer = p_id_organizer
            WHERE id_location = p_id_location;

            -- Ensure the update affected rows
            IF SQL%ROWCOUNT = 0 THEN
                RAISE_APPLICATION_ERROR(-20001, 'No location found with the given ID.');
            END IF;
        END IF;

        -- Commit the transaction
        COMMIT;
    END persist_location;

    -- DELETE LOCATION
    PROCEDURE delete_location (
        p_id_location IN NUMBER
    ) IS
    BEGIN
        
        DELETE FROM locations WHERE id_location = p_id_location;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20002, 'Location with ID ' || p_id_location || ' does not exist.');
        END IF;

        DBMS_OUTPUT.PUT_LINE('Location deleted successfully.');
        COMMIT;
    END delete_location;

END locations_pkg;