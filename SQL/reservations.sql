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

    PROCEDURE batch_process_location_requests (
        p_id_location IN locations.id_location%TYPE
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
        v_existing_notification_count NUMBER;
    BEGIN
        -- Získej ID typů notifikací
        SELECT id_notification_type INTO v_success_type_id 
        FROM notification_types WHERE "code" = 'ALLOC_SUCCESS';
        
        SELECT id_notification_type INTO v_fail_type_id 
        FROM notification_types WHERE "code" = 'ALLOC_FAIL';

        -- kdo podal žádost
        SELECT id_organizer INTO v_organizer_id
        FROM room_requests
        WHERE id_room_request = p_id_room_request;

        -- VLOŽ REZERVACI
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

        -- KONTROLA: Existuje už notifikace?
        SELECT COUNT(*) INTO v_existing_notification_count
        FROM notifications
        WHERE id_room_request = p_id_room_request;

        -- NOVÁ LOGIKA: Vytvoř nebo aktualizuj
        IF v_existing_notification_count = 0 THEN
            -- NOVÁ NOTIFIKACE
            IF v_rows > 0 THEN
                INSERT INTO notifications (id_notification_type, delivered, id_organizer, id_room_request)
                VALUES (v_success_type_id, 'N', v_organizer_id, p_id_room_request);
                DBMS_OUTPUT.PUT_LINE('✅ Vytvořena notifikace SUCCESS pro request ' || p_id_room_request);
            ELSE
                INSERT INTO notifications (id_notification_type, delivered, id_organizer, id_room_request)
                VALUES (v_fail_type_id, 'N', v_organizer_id, p_id_room_request);
                DBMS_OUTPUT.PUT_LINE('❌ Vytvořena notifikace FAIL pro request ' || p_id_room_request);
            END IF;
        ELSE
            -- AKTUALIZACE EXISTUJÍCÍ
            IF v_rows > 0 THEN
                UPDATE notifications
                SET id_notification_type = v_success_type_id,
                    delivered = 'N'  -- Resetuj delivered → uživatel musí vidět změnu
                WHERE id_room_request = p_id_room_request;
                
                DBMS_OUTPUT.PUT_LINE('Notifikace změněna na SUCCESS (nepřečtená) pro request ' || p_id_room_request);
            END IF;
        END IF;
    END process_request;

    -- NOVÁ DÁVKOVÁ PROCEDURA
    PROCEDURE batch_process_location_requests (
        p_id_location IN locations.id_location%TYPE
    ) IS
        v_processed_count NUMBER := 0;
        v_success_count NUMBER := 0;
        v_fail_count NUMBER := 0;
        
        CURSOR unallocated_requests_cur IS
            SELECT 
                rr.id_room_request,
                rr."type",
                COALESCE(mr.vc_ready, 'N') AS vc_ready,
                pr.podium_size
            FROM room_requests rr
            LEFT JOIN meeting_rrequests mr ON mr.id_room_request = rr.id_room_request
            LEFT JOIN presentation_rrequests pr ON pr.id_room_request = rr.id_room_request
            WHERE rr.id_location = p_id_location
            AND NOT EXISTS (
                SELECT 1 
                FROM reservations res 
                WHERE res.id_room_request = rr.id_room_request
            )
            ORDER BY rr.reservation_start;
    BEGIN
        DBMS_OUTPUT.PUT_LINE('=== DÁVKOVÉ ZPRACOVÁNÍ LOKACE ' || p_id_location || ' ===');
        
        FOR req IN unallocated_requests_cur LOOP
            BEGIN
                v_processed_count := v_processed_count + 1;
                
                -- Pokus o alokaci
                process_request(
                    p_id_room_request => req.id_room_request,
                    p_type            => req."type",
                    p_vc_ready        => req.vc_ready,
                    p_podium_size     => req.podium_size
                );
                
                -- Kontrola, zda byla vytvořena rezervace
                DECLARE
                    v_reservation_exists NUMBER;
                BEGIN
                    SELECT COUNT(*) INTO v_reservation_exists
                    FROM reservations
                    WHERE id_room_request = req.id_room_request;
                    
                    IF v_reservation_exists > 0 THEN
                        v_success_count := v_success_count + 1;
                        DBMS_OUTPUT.PUT_LINE('Request ' || req.id_room_request || ' úspěšně alokován');
                    ELSE
                        v_fail_count := v_fail_count + 1;
                        DBMS_OUTPUT.PUT_LINE('Request ' || req.id_room_request || ' nelze alokovat');
                    END IF;
                END;
                
            EXCEPTION
                WHEN OTHERS THEN
                    v_fail_count := v_fail_count + 1;
                    DBMS_OUTPUT.PUT_LINE('Chyba při zpracování requestu ' || req.id_room_request || ': ' || SQLERRM);
                    -- Pokračujeme dále
            END;
        END LOOP;
        
        DBMS_OUTPUT.PUT_LINE('=== SOUHRN ===');
        DBMS_OUTPUT.PUT_LINE('Zpracováno: ' || v_processed_count);
        DBMS_OUTPUT.PUT_LINE('Úspěch: ' || v_success_count);
        DBMS_OUTPUT.PUT_LINE('Selhání: ' || v_fail_count);
        
        -- ODSTRANĚNO: COMMIT; 
        -- Commit provede trigger
        
    EXCEPTION
        WHEN OTHERS THEN
            -- ODSTRANĚNO: ROLLBACK;
            DBMS_OUTPUT.PUT_LINE('KRITICKÁ CHYBA: ' || SQLERRM);
            RAISE;
    END batch_process_location_requests;

END reservations_pkg;
