CREATE OR REPLACE PACKAGE reservations_pkg AS
    PROCEDURE edit_request (
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


     -- procedura pro alokaci místnosti k požadavku
    PROCEDURE process_request (
        p_id_room_request IN room_requests.id_room_request%TYPE,
        p_type            IN VARCHAR2,
        p_vc_ready        IN CHAR    DEFAULT NULL,
        p_podium_size     IN NUMBER  DEFAULT NULL
    );

    PROCEDURE mark_notification_delivered(
    p_id_organizer IN NUMBER,
    p_notification_id IN NUMBER  -- JEN JEDNO ID
);
    
    -- ========================================
    -- FUNKCE PRO NOTIFIKACE
    -- ========================================

    -- Získání počtu nepřečtených notifikací pro organizátora
    FUNCTION get_unread_notification_count(
        p_id_organizer IN NUMBER
    ) RETURN NUMBER;

PROCEDURE get_organizers_hierarchy(
    p_cursor OUT SYS_REFCURSOR
);

END reservations_pkg;
/

CREATE OR REPLACE PACKAGE BODY reservations_pkg AS

    PROCEDURE edit_request (
        p_id_room_request IN NUMBER DEFAULT NULL,
        p_min_capacity IN NUMBER,
        p_type IN VARCHAR2,
        p_reservation_start IN DATE,
        p_reservation_end IN DATE,
        p_reservation_length IN DATE, -- Now treated directly as DATE
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
    END edit_request;

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
    

-- Automatická alokace místnosti bez sloupce STATUS v ROOM_REQUESTS
-- PŘEPIS / REPLACE

PROCEDURE process_request (
    p_id_room_request IN room_requests.id_room_request%TYPE,
    p_type            IN VARCHAR2,
    p_vc_ready        IN CHAR    DEFAULT NULL,
    p_podium_size     IN NUMBER  DEFAULT NULL
) IS
    v_organizer_id room_requests.id_organizer%TYPE;
    v_rows NUMBER;
BEGIN
    -- kdo podal žádost
    SELECT id_organizer
      INTO v_organizer_id
      FROM room_requests
     WHERE id_room_request = p_id_room_request;

    -- VLOŽ REZERVACI – bez čtení z mutující tabulky podtypu
    INSERT INTO reservations ("start", "end", id_room, id_room_request, id_organizer)
    SELECT req.reservation_start,
           req.reservation_end,
           alloc.id_room,
           req.id_room_request,
           req.id_organizer
      FROM room_requests req
      JOIN (
            SELECT inner_req.id_room_request,
                   rm.id_room,
                   ROW_NUMBER() OVER (PARTITION BY inner_req.id_room_request ORDER BY rm.id_room) rn
              FROM room_requests inner_req
              JOIN rooms rm
                ON rm.capacity >= inner_req.min_capacity
               AND (inner_req.id_location IS NULL OR rm.id_location = inner_req.id_location)
               AND (
                    (p_type = 'MEETING_RREQUEST' AND EXISTS (
                         SELECT 1
                           FROM meeting_rooms mr
                          WHERE mr.id_room = rm.id_room
                            AND (p_vc_ready = 'N' OR (p_vc_ready = 'Y' AND mr.vc_ready = 'Y'))
                    )))
                 OR (p_type = 'PRESENTATION_RREQUEST' AND EXISTS (
                         SELECT 1
                           FROM presentation_rooms pr
                          WHERE pr.id_room = rm.id_room
                            AND p_podium_size <= pr.podium_size
                    )
                )
               WHERE inner_req.id_room_request = p_id_room_request
               AND NOT EXISTS (
                    SELECT 1
                      FROM reservations r
                     WHERE r.id_room = rm.id_room
                       AND r."start" < inner_req.reservation_end
                       AND r."end"   > inner_req.reservation_start
               )
           ) alloc
        ON alloc.id_room_request = req.id_room_request
     WHERE req.id_room_request = p_id_room_request
       AND alloc.rn = 1;

    v_rows := SQL%ROWCOUNT;

    IF v_rows > 0 THEN
        INSERT INTO notifications (notification_type, delivered, id_organizer, id_room_request)
        VALUES ('ALLOC_SUCCESS', 'N', v_organizer_id, p_id_room_request);
    ELSE
        INSERT INTO notifications (notification_type, delivered, id_organizer, id_room_request)
        VALUES ('ALLOC_FAIL', 'N', v_organizer_id, p_id_room_request);
    END IF;
END process_request;


    -- ========================================
    -- FUNKCE PRO NOTIFIKACE
    -- ========================================

-- Získání počtu nepřečtených notifikací pro organizátora
FUNCTION get_unread_notification_count(
    p_id_organizer IN NUMBER
) RETURN NUMBER
IS
    v_count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_count
    FROM notifications
    WHERE id_organizer = p_id_organizer
      AND delivered = 'N';
        
    RETURN v_count;
EXCEPTION
    WHEN OTHERS THEN
        RETURN 0;
END get_unread_notification_count;

    PROCEDURE mark_notification_delivered(
    p_id_organizer IN NUMBER,
    p_notification_id IN NUMBER  -- JEN JEDNO ID
) IS
BEGIN
    UPDATE notifications
    SET delivered = 'Y'
    WHERE id_notification = p_notification_id
      AND id_organizer = p_id_organizer;
    
    COMMIT;
END mark_notification_delivered;

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

END reservations_pkg;
/



/*
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
*/

--BEGIN
--    reservations_pkg.edit_request(
--        p_id_room_request => 1, -- Update request with ID 1
--        p_min_capacity => 20,
--        p_type => 'PRESENTATION_RREQUEST',
--        p_reservation_start => TO_DATE('2024-12-08 10:00:00', 'YYYY-MM-DD HH24:MI:SS'),
--        p_reservation_end => TO_DATE('2024-12-08 12:00:00', 'YYYY-MM-DD HH24:MI:SS'),
--        p_reservation_length => TO_DATE('1000-01-01 01:30:00', 'YYYY-MM-DD HH24:MI:SS'),
--        p_id_location => 2,
--        p_id_organizer => 2,
--        p_podium_size => 10
--    );
--END;
--/


--BEGIN
--    reservations_pkg.edit_request(
--        p_id_room_request => NULL,
--        p_min_capacity => 20,
--        p_type => 'MEETING_RREQUEST',
--        p_reservation_start => TO_DATE('2024-12-07 09:00:00', 'YYYY-MM-DD HH24:MI:SS'),
--        p_reservation_end => TO_DATE('2024-12-07 11:00:00', 'YYYY-MM-DD HH24:MI:SS'),
--        p_reservation_length => TO_DATE('1000-01-01 02:00:00', 'YYYY-MM-DD HH24:MI:SS'),
--        p_id_location => 1,
--        p_id_organizer => 1,
--        p_vc_ready => 'Y'
--    );
--END;
--/

