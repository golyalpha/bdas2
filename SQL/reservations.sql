CREATE OR REPLACE PACKAGE reservations_pkg AS

    PROCEDURE persist_reservation(
        p_id_reservation IN NUMBER DEFAULT NULL,
        p_start IN DATE,
        p_end IN DATE,
        p_id_room IN NUMBER,
        p_id_room_request IN NUMBER,
        p_id_organizer IN NUMBER
    );

    PROCEDURE delete_reservation (
        p_id_reservation IN NUMBER
    );

    -- procedura pro alokaci místnosti k požadavku
    PROCEDURE process_request (
        p_id_room_request IN room_requests.id_room_request%TYPE,
        p_type            IN VARCHAR2,
        p_vc_ready        IN CHAR    DEFAULT NULL,
        p_podium_size     IN NUMBER  DEFAULT NULL
    );

END reservations_pkg;
/

CREATE OR REPLACE PACKAGE BODY reservations_pkg AS

    PROCEDURE persist_reservation(
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
    END persist_reservation;

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

    PROCEDURE process_request (
        p_id_room_request IN room_requests.id_room_request%TYPE,
        p_type            IN VARCHAR2,
        p_vc_ready        IN CHAR    DEFAULT NULL,
        p_podium_size     IN NUMBER  DEFAULT NULL
    ) IS
        v_organizer_id room_requests.id_organizer%TYPE;
        v_rows NUMBER;
        v_success_type_id notification_types.id_notification_type%TYPE;
        v_fail_type_id notification_types.id_notification_type%TYPE;
    BEGIN
        -- Získej ID typů notifikací
        SELECT id_notification_type INTO v_success_type_id 
        FROM notification_types WHERE "code" = 'ALLOC_SUCCESS';
        
        SELECT id_notification_type INTO v_fail_type_id 
        FROM notification_types WHERE "code" = 'ALLOC_FAIL';


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
            INSERT INTO notifications (id_notification_type, delivered, id_organizer, id_room_request)
            VALUES (v_success_type_id, 'N', v_organizer_id, p_id_room_request);
        ELSE
            INSERT INTO notifications (id_notification_type, delivered, id_organizer, id_room_request)
            VALUES (v_fail_type_id, 'N', v_organizer_id, p_id_room_request);
        END IF;
    END process_request;

END reservations_pkg;
/
