CREATE OR REPLACE PACKAGE requests_pkg AS

    PROCEDURE persist_request (
        p_id_room_request IN NUMBER DEFAULT NULL,
        p_min_capacity IN NUMBER,
        p_type IN VARCHAR2,
        p_reservation_start IN DATE,
        p_reservation_end IN DATE,
        p_reservation_length IN DATE,
        p_id_location IN NUMBER,
        p_id_organizer IN NUMBER,
        p_vc_ready IN CHAR DEFAULT NULL,
        p_podium_size IN NUMBER DEFAULT NULL
    );

        PROCEDURE delete_request (
        p_id_room_request IN NUMBER
    );

END requests_pkg;
/

CREATE OR REPLACE PACKAGE BODY requests_pkg AS 

    -- DELETE ROOM REQUEST
    PROCEDURE delete_request (
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
    END delete_request;

    PROCEDURE persist_request (
        p_id_room_request IN NUMBER DEFAULT NULL,
        p_min_capacity IN NUMBER,
        p_type IN VARCHAR2,
        p_reservation_start IN DATE,
        p_reservation_end IN DATE,
        p_reservation_length IN DATE,
        p_id_location IN NUMBER,
        p_id_organizer IN NUMBER,
        p_vc_ready IN CHAR DEFAULT NULL,
        p_podium_size IN NUMBER DEFAULT NULL
    ) IS
        v_new_id NUMBER;
    BEGIN
        IF p_id_room_request IS NULL THEN
            -- Generate new ID using sequence
            SELECT room_requests_id_room_request.NEXTVAL INTO v_new_id FROM dual;

            -- Insert into the base room_requests table
            INSERT INTO room_requests (
                id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer
            ) VALUES (
                v_new_id, p_min_capacity, p_type, p_reservation_start, p_reservation_end, 
                p_reservation_length, -- Store directly as DATE
                p_id_location, p_id_organizer
            );

            -- Handle subtypes
            IF p_type = 'MEETING_RREQUEST' THEN
                INSERT INTO meeting_rrequests (id_room_request, vc_ready)
                VALUES (v_new_id, p_vc_ready);
            ELSIF p_type = 'PRESENTATION_RREQUEST' THEN
                INSERT INTO presentation_rrequests (id_room_request, podium_size)
                VALUES (v_new_id, p_podium_size);
            END IF;

        ELSE
            -- Update the base room_requests table
            UPDATE room_requests
            SET min_capacity = p_min_capacity,
                "type" = p_type,
                reservation_start = p_reservation_start,
                reservation_end = p_reservation_end,
                reservation_length = p_reservation_length, -- Store directly as DATE
                id_location = p_id_location,
                id_organizer = p_id_organizer
            WHERE id_room_request = p_id_room_request;

            -- Handle subtypes
            IF p_type = 'MEETING_RREQUEST' THEN
                MERGE INTO meeting_rrequests m
                USING (SELECT p_id_room_request AS id_room_request FROM dual) d
                ON (m.id_room_request = d.id_room_request)
                WHEN MATCHED THEN
                    UPDATE SET vc_ready = p_vc_ready
                WHEN NOT MATCHED THEN
                    INSERT (id_room_request, vc_ready) VALUES (p_id_room_request, p_vc_ready);
            ELSIF p_type = 'PRESENTATION_RREQUEST' THEN
                MERGE INTO presentation_rrequests p
                USING (SELECT p_id_room_request AS id_room_request FROM dual) d
                ON (p.id_room_request = d.id_room_request)
                WHEN MATCHED THEN
                    UPDATE SET podium_size = p_podium_size
                WHEN NOT MATCHED THEN
                    INSERT (id_room_request, podium_size) VALUES (p_id_room_request, p_podium_size);
            END IF;
        END IF;

        COMMIT;
    END persist_request;

END requests_pkg;
