CREATE OR REPLACE PACKAGE reservations_pkg AS
    -- CREATE procedure
    PROCEDURE create_room (
        p_name IN VARCHAR2,
        p_capacity IN NUMBER,
        p_type IN VARCHAR2,
        p_id_location IN NUMBER,
        p_id_organizer IN NUMBER,
        p_vc_ready IN CHAR DEFAULT NULL, -- CHAR(1): 'Y' or 'N'
        p_podium_size IN NUMBER DEFAULT NULL -- Pro PRESENTATION_ROOM
    );

    -- UPDATE procedure
    PROCEDURE update_room (
        p_id_room IN NUMBER,
        p_name IN VARCHAR2,
        p_capacity IN NUMBER,
        p_type IN VARCHAR2,
        p_id_location IN NUMBER,
        p_id_organizer IN NUMBER
    );

    -- DELETE procedure
    PROCEDURE delete_room (
        p_id_room IN NUMBER
    );
END reservations_pkg;
/

CREATE OR REPLACE PACKAGE BODY reservations_pkg AS

    -- CREATE ROOM
    PROCEDURE create_room (
        p_name IN VARCHAR2,
        p_capacity IN NUMBER,
        p_type IN VARCHAR2,
        p_id_location IN NUMBER,
        p_id_organizer IN NUMBER,
        p_vc_ready IN CHAR DEFAULT NULL, -- CHAR(1)
        p_podium_size IN NUMBER DEFAULT NULL
    ) IS
        v_id_room NUMBER;
    BEGIN
        -- Ověření typu místnosti a požadovaných parametrů
        IF p_type NOT IN ('MEETING_ROOM', 'PRESENTATION_ROOM') THEN
            RAISE_APPLICATION_ERROR(-20003, 'Invalid room type. Must be MEETING_ROOM or PRESENTATION_ROOM.');
        END IF;

        IF p_type = 'MEETING_ROOM' AND p_vc_ready IS NULL THEN
            RAISE_APPLICATION_ERROR(-20004, 'vc_ready parameter is required for MEETING_ROOM.');
        ELSIF p_type = 'PRESENTATION_ROOM' AND p_podium_size IS NULL THEN
            RAISE_APPLICATION_ERROR(-20005, 'podium_size parameter is required for PRESENTATION_ROOM.');
        END IF;

        -- Vygenerování nového ID místnosti
        SELECT rooms_id_room_seq.NEXTVAL INTO v_id_room FROM dual;

        -- Vložení do základní tabulky rooms
        INSERT INTO rooms (
            id_room, name, capacity, type, id_location, id_organizer
        ) VALUES (
            v_id_room, p_name, p_capacity, p_type, p_id_location, p_id_organizer
        );

        -- Vložení do podtypové tabulky podle typu
        IF p_type = 'MEETING_ROOM' THEN
            INSERT INTO meeting_rooms (id_room, vc_ready) 
            VALUES (v_id_room, p_vc_ready);
        ELSIF p_type = 'PRESENTATION_ROOM' THEN
            INSERT INTO presentation_rooms (id_room, podium_size) 
            VALUES (v_id_room, p_podium_size);
        END IF;

        DBMS_OUTPUT.PUT_LINE('Room created successfully with ID: ' || v_id_room);
        COMMIT;
    END create_room;

    -- UPDATE ROOM (neměníme)
    PROCEDURE update_room (
        p_id_room IN NUMBER,
        p_name IN VARCHAR2,
        p_capacity IN NUMBER,
        p_type IN VARCHAR2,
        p_id_location IN NUMBER,
        p_id_organizer IN NUMBER
    ) IS
    BEGIN
        UPDATE rooms
        SET name = p_name,
            capacity = p_capacity,
            type = p_type,
            id_location = p_id_location,
            id_organizer = p_id_organizer
        WHERE id_room = p_id_room;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20001, 'Room with ID ' || p_id_room || ' does not exist.');
        END IF;

        DBMS_OUTPUT.PUT_LINE('Room updated successfully.');
        COMMIT;
    END update_room;

    -- DELETE ROOM
    PROCEDURE delete_room (
        p_id_room IN NUMBER
    ) IS
    BEGIN
        DELETE FROM meeting_rooms WHERE id_room = p_id_room;
        DELETE FROM presentation_rooms WHERE id_room = p_id_room;

        DELETE FROM rooms WHERE id_room = p_id_room;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20002, 'Room with ID ' || p_id_room || ' does not exist.');
        END IF;

        DBMS_OUTPUT.PUT_LINE('Room deleted successfully.');
        COMMIT;
    END delete_room;

END reservations_pkg;
/


BEGIN
    reservations_pkg.create_room(
        p_name => 'Meeting Room 10',
        p_capacity => 50,
        p_type => 'MEETING_ROOM',
        p_id_location => 1,
        p_id_organizer => 2,
        p_vc_ready => 'Y' -- CHAR(1): 'Y' nebo 'N'
    );
END;
/

BEGIN
    reservations_pkg.create_room(
        p_name => 'Presentation Room 202',
        p_capacity => 100,
        p_type => 'PRESENTATION_ROOM',
        p_id_location => 2,
        p_id_organizer => 3,
        p_podium_size => 25 -- Velikost podia
    );
END;
/

BEGIN
    reservations_pkg.create_room(
        p_name => 'Presentation Room 202',
        p_capacity => 100,
        p_type => 'PRESENTATION_ROOM',
        p_id_location => 2,
        p_id_organizer => 3,
        p_podium_size => 25
    );
END;
/


BEGIN
    reservations_pkg.update_room(
        p_id_room => 1,
        p_name => 'Meeting Room kjadsfh',
        p_capacity => 50,
        p_type => 'MEETING_ROOM',
        p_id_location => 1,
        p_id_organizer => 2
    );
END;

BEGIN
    reservations_pkg.delete_room(1);
END;

