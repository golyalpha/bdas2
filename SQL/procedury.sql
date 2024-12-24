CREATE OR REPLACE PACKAGE reservations_pkg AS
    PROCEDURE edit_room (
        p_id_room IN NUMBER DEFAULT NULL,
        p_name IN VARCHAR2,
        p_capacity IN NUMBER,
        p_type IN VARCHAR2,
        p_id_location IN NUMBER,
        p_id_organizer IN NUMBER,
        p_vc_ready IN CHAR DEFAULT NULL, -- pro MEETING_ROOM
        p_podium_size IN NUMBER DEFAULT NULL -- pro PRESENTATION_ROOM
    );

    PROCEDURE edit_organizer(
        p_id_organizer IN NUMBER DEFAULT NULL,
        p_name IN VARCHAR2,
        p_email IN VARCHAR2,
        p_id_role IN NUMBER DEFAULT NULL,
        p_id_organizer_substitute IN NUMBER DEFAULT NULL
    );

    PROCEDURE edit_credential(
        p_id_credential IN NUMBER DEFAULT NULL,
        p_credential_type IN CHAR,
        p_data IN VARCHAR2,
        p_id_organizer IN NUMBER
    );

    PROCEDURE edit_reservation(
        p_id_reservation IN NUMBER DEFAULT NULL,
        p_start IN DATE,
        p_end IN DATE,
        p_id_room IN NUMBER,
        p_id_room_request IN NUMBER,
        p_id_organizer IN NUMBER
    );

    PROCEDURE edit_location(
        p_id_location IN NUMBER DEFAULT NULL,
        p_name IN VARCHAR2,
        p_start IN DATE,
        p_end IN DATE,
        p_id_city IN NUMBER,
        p_id_organizer IN NUMBER
    );

    -- DELETE procedures
    PROCEDURE delete_room (
        p_id_room IN NUMBER
    );

    PROCEDURE delete_location (
        p_id_location IN NUMBER
    );

    PROCEDURE delete_reservation (
        p_id_reservation IN NUMBER
    );

    PROCEDURE delete_room_request (
        p_id_room_request IN NUMBER
    );

END reservations_pkg;
/

CREATE OR REPLACE PACKAGE BODY reservations_pkg AS
    -- Edit or Create Location
    PROCEDURE edit_location(
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
    END edit_location;

    -- EDIT ROOM
    PROCEDURE edit_room (
        p_id_room IN NUMBER DEFAULT NULL,
        p_name IN VARCHAR2,
        p_capacity IN NUMBER,
        p_type IN VARCHAR2,
        p_id_location IN NUMBER,
        p_id_organizer IN NUMBER,
        p_vc_ready IN CHAR DEFAULT NULL,
        p_podium_size IN NUMBER DEFAULT NULL
    ) IS
        v_new_id NUMBER;
    BEGIN
        -- Kontrola typu místnosti
        IF p_type NOT IN ('MEETING_ROOM', 'PRESENTATION_ROOM') THEN
            RAISE_APPLICATION_ERROR(-20003, 'Invalid room type.');
        END IF;

        -- Pro nový záznam
        IF p_id_room IS NULL OR p_id_room = 0 THEN
            SELECT rooms_id_room_seq.NEXTVAL INTO v_new_id FROM dual;

            INSERT INTO rooms (id_room, "name", capacity, "type", id_location, id_organizer)
            VALUES (v_new_id, p_name, p_capacity, p_type, p_id_location, p_id_organizer);

            IF p_type = 'MEETING_ROOM' THEN
                INSERT INTO meeting_rooms (id_room, vc_ready)
                VALUES (v_new_id, p_vc_ready);
            ELSIF p_type = 'PRESENTATION_ROOM' THEN
                INSERT INTO presentation_rooms (id_room, podium_size)
                VALUES (v_new_id, p_podium_size);
            END IF;

        -- Pro aktualizaci
        ELSE
            UPDATE rooms
            SET "name" = p_name,
                capacity = p_capacity,
                "type" = p_type,
                id_location = p_id_location,
                id_organizer = p_id_organizer
            WHERE id_room = p_id_room;

            IF p_type = 'MEETING_ROOM' THEN
                UPDATE meeting_rooms
                SET vc_ready = p_vc_ready
                WHERE id_room = p_id_room;
            ELSIF p_type = 'PRESENTATION_ROOM' THEN
                UPDATE presentation_rooms
                SET podium_size = p_podium_size
                WHERE id_room = p_id_room;
            END IF;
        END IF;

        COMMIT;
    END edit_room;

    -- EDIT RESERVATION
PROCEDURE edit_reservation(
    p_id_reservation IN NUMBER DEFAULT NULL, 
    p_start IN DATE,
    p_end IN DATE,
    p_id_room IN NUMBER,
    p_id_room_request IN NUMBER,
    p_id_organizer IN NUMBER
) IS
    v_new_id NUMBER; -- Variable to store new ID
BEGIN
    -- If p_id_reservation is NULL, insert a new reservation
    IF p_id_reservation IS NULL THEN
        SELECT reservations_id_reservation.NEXTVAL INTO v_new_id FROM dual;

        INSERT INTO reservations (
            id_reservation,
            "start",
            "end",
            id_room,
            id_room_request,
            id_organizer
        ) VALUES (
            v_new_id,
            p_start,
            p_end,
            p_id_room,
            p_id_room_request,
            p_id_organizer
        );
    ELSE
        -- Otherwise, update the existing reservation
        UPDATE reservations
        SET 
            "start" = p_start,
            "end" = p_end,
            id_room = p_id_room,
            id_room_request = p_id_room_request,
            id_organizer = p_id_organizer
        WHERE id_reservation = p_id_reservation;

        -- Confirm the update occurred
        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20001, 'No reservation found with the given ID.');
        END IF;
    END IF;

    COMMIT;
END edit_reservation;

PROCEDURE edit_organizer (
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
                id_role = p_id_role,
                id_organizer_substitute = p_id_organizer_substitute
            WHERE id_organizer = p_id_organizer;

            IF SQL%ROWCOUNT = 0 THEN
                RAISE_APPLICATION_ERROR(-20001, 'No organizer found with the given ID.');
            END IF;
        END IF;

        COMMIT;
    END edit_organizer;

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

    -- DELETE RESERVATION
    PROCEDURE delete_reservation (
        p_id_reservation IN NUMBER
    ) IS
    BEGIN
        DELETE FROM reservations WHERE id_reservation = p_id_reservation;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20002, 'Reservation with ID ' || p_id_reservation || ' does not exist.');
        END IF;

        DBMS_OUTPUT.PUT_LINE('Reservation deleted successfully.');
        COMMIT;
    END delete_reservation;


    -- DELETE ROOM REQUEST
    PROCEDURE delete_room_request (
        p_id_room_request IN NUMBER
    ) IS
    BEGIN
        DELETE FROM PRESENTATION_RREQUESTS WHERE id_room_request = p_id_room_request;
        DELETE FROM MEETING_RREQUESTS WHERE id_room_request = p_id_room_request;

        DELETE FROM room_requests WHERE id_room_request = p_id_room_request;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20002, 'Room request with ID ' || p_id_room_request || ' does not exist.');
        END IF;

        DBMS_OUTPUT.PUT_LINE('Room request deleted successfully.');
        COMMIT;
    END delete_room_request;

    PROCEDURE edit_credential(
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
    END edit_credential;
END reservations_pkg;
/

BEGIN
    reservations_pkg.edit_location(
        p_id_location => 0,
        p_name => 'aaaadddddddddddaaaal',
        p_start => TO_DATE('2024-12-06 10:00', 'YYYY-MM-DD HH24:MI'),
        p_end => TO_DATE('2024-12-06 19:00', 'YYYY-MM-DD HH24:MI'),
        p_id_city => 3,
        p_id_organizer => 3
    );
END;
/


BEGIN
    RESERVATIONS_PKG.EDIT_ROOM(
        P_ID_ROOM => NULL,
        P_NAME => 'Room 10DAHFKJAFHDK1',
        P_CAPACITY => 50,
        P_TYPE => 'MEETING_ROOM',
        P_ID_LOCATION => 1,
        P_ID_ORGANIZER => 1,
        P_VC_READY => 'Y'
    );
END;
/




BEGIN
    reservations_pkg.edit_reservation(
        p_id_reservation => NULL,
        p_start => TO_DATE('2024-01-10 14:00:00' , 'YYYY-MM-DD HH24:MI:SS'),
        p_end => TO_DATE('2024-01-10 16:00:00', 'YYYY-MM-DD HH24:MI:SS'),
        p_id_room => 1,
        p_id_room_request => 1,
        p_id_organizer => 1
    );
END;
/





BEGIN
    reservations_pkg.edit_reservation(
        p_id_reservation => NULL,   -- Pass NULL if inserting a new reservation
        p_start => SYSDATE,        -- Replace with a valid DATE value
        p_end => SYSDATE + 2,      -- Replace with a valid DATE value for end
        p_id_room => 1,            -- Replace with a valid Room ID
        p_id_room_request => 1,    -- Replace with a valid Room Request ID
        p_id_organizer => 1        -- Replace with a valid Organizer ID
    );
END;
/
