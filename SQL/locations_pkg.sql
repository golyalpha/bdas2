CREATE OR REPLACE PACKAGE locations_pkg AS
    -- Public proměnná pro detekci cancel operace
    g_cancel_in_progress BOOLEAN := FALSE;
    
    PROCEDURE persist_location(
        p_id_location IN NUMBER,
        p_name IN VARCHAR2,
        p_availability_start IN DATE,
        p_availability_end IN DATE,
        p_id_city IN NUMBER,
        p_id_organizer IN NUMBER
    );
    
    PROCEDURE delete_location(
        p_id_location IN NUMBER
    );
END locations_pkg;
/

CREATE OR REPLACE PACKAGE BODY locations_pkg AS

    -- Pomocná procedura: Zrušení žádosti mimo dostupnost
    PROCEDURE cancel_invalid_requests(
        p_id_location IN NUMBER,
        p_new_start IN DATE,
        p_new_end IN DATE
    ) IS
        v_count_cancelled NUMBER := 0;
        v_id_notification_type_fail NUMBER;
        
        CURSOR c_invalid_requests IS
            SELECT rr.id_room_request, rr.id_organizer, rr.reservation_start, rr.reservation_end
            FROM room_requests rr
            WHERE rr.id_location = p_id_location
              AND (
                  TO_CHAR(rr.reservation_start, 'HH24:MI') < TO_CHAR(p_new_start, 'HH24:MI')
                  OR
                  TO_CHAR(rr.reservation_end, 'HH24:MI') > TO_CHAR(p_new_end, 'HH24:MI')
              );
    BEGIN
        DBMS_OUTPUT.PUT_LINE('=== KONTROLA NEPLATNÝCH ŽÁDOSTÍ ===');
        DBMS_OUTPUT.PUT_LINE('Nové rozmezí: ' || TO_CHAR(p_new_start, 'HH24:MI') || ' - ' || TO_CHAR(p_new_end, 'HH24:MI'));
        
        -- NASTAVÍME PŘÍZNAK, ŽE PROBÍHÁ CANCEL
        g_cancel_in_progress := TRUE;
        
        -- Získání ID typu notifikace
        BEGIN
            SELECT id_notification_type INTO v_id_notification_type_fail
            FROM notification_types
            WHERE "code" = 'ALLOC_FAIL';
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                DBMS_OUTPUT.PUT_LINE('Typ notifikace ALLOC_FAIL nenalezen!');
                g_cancel_in_progress := FALSE;
                RETURN;
        END;
        
        -- Procházení neplatných žádostí
        FOR rec IN c_invalid_requests LOOP
            v_count_cancelled := v_count_cancelled + 1;
            
            DBMS_OUTPUT.PUT_LINE('Rušení žádosti ID=' || rec.id_room_request);
            
            BEGIN
                -- KROK 1: Smazání rezervace (trigger SE NESPUSTÍ díky příznaku)
                DECLARE
                    v_reservation_exists NUMBER;
                BEGIN
                    SELECT COUNT(*) INTO v_reservation_exists
                    FROM reservations
                    WHERE id_room_request = rec.id_room_request;
                    
                    IF v_reservation_exists > 0 THEN
                        DBMS_OUTPUT.PUT_LINE('Mazání rezervace...');
                        DELETE FROM reservations WHERE id_room_request = rec.id_room_request;
                    END IF;
                END;
                
                -- KROK 2: Notifikace
                DECLARE
                    v_notification_exists NUMBER;
                BEGIN
                    SELECT COUNT(*) INTO v_notification_exists
                    FROM notifications
                    WHERE id_room_request = rec.id_room_request;
                    
                    IF v_notification_exists > 0 THEN
                        UPDATE notifications SET
                            id_notification_type = v_id_notification_type_fail,
                            delivered = 'N'
                        WHERE id_room_request = rec.id_room_request;
                        DBMS_OUTPUT.PUT_LINE('Notifikace aktualizována');
                    ELSE
                        INSERT INTO notifications (
                            id_notification,
                            delivered,
                            id_organizer,
                            id_room_request,
                            id_notification_type
                        ) VALUES (
                            notifications_id_notification.NEXTVAL,
                            'N',
                            rec.id_organizer,
                            rec.id_room_request,
                            v_id_notification_type_fail
                        );
                        DBMS_OUTPUT.PUT_LINE('Notifikace vytvořena');
                    END IF;
                END;
                
                -- KROK 3: Smazání žádosti
                DELETE FROM room_requests WHERE id_room_request = rec.id_room_request;
                DBMS_OUTPUT.PUT_LINE('Žádost zrušena');
                
            EXCEPTION
                WHEN OTHERS THEN
                    DBMS_OUTPUT.PUT_LINE('Chyba: ' || SQLERRM);
            END;
        END LOOP;
        
        -- RESETUJEME PŘÍZNAK
        g_cancel_in_progress := FALSE;
        
        IF v_count_cancelled > 0 THEN
            DBMS_OUTPUT.PUT_LINE('=== ZRUŠENO ' || v_count_cancelled || ' ŽÁDOSTÍ ===');
            
            -- NYNÍ SPUSTÍME REALOKACI RUČNĚ (pouze jednou)
            DBMS_OUTPUT.PUT_LINE('Spouštím realokaci pro lokaci ID: ' || p_id_location);
            reservations_pkg.batch_process_location_requests(p_id_location => p_id_location);
        ELSE
            DBMS_OUTPUT.PUT_LINE('=== ŽÁDNÉ ŽÁDOSTI K ZRUŠENÍ ===');
        END IF;
        
    EXCEPTION
        WHEN OTHERS THEN
            g_cancel_in_progress := FALSE;  --  Vždy resetujeme
            DBMS_OUTPUT.PUT_LINE('KRITICKÁ CHYBA: ' || SQLERRM);
            RAISE;
    END cancel_invalid_requests;

    -- Hlavní procedura: Uložení/Úprava budovy
    PROCEDURE persist_location(
        p_id_location IN NUMBER,
        p_name IN VARCHAR2,
        p_availability_start IN DATE,
        p_availability_end IN DATE,
        p_id_city IN NUMBER,
        p_id_organizer IN NUMBER
    ) IS
        v_old_start DATE;
        v_old_end DATE;
    BEGIN
        DBMS_OUTPUT.PUT_LINE('=== PERSIST_LOCATION START ===');
        
        IF p_id_location IS NULL THEN
            DBMS_OUTPUT.PUT_LINE('ID je NULL - INSERT');
            
            INSERT INTO locations (
                id_location,
                "name",
                availability_start,
                availability_end,
                id_city,
                id_organizer
            ) VALUES (
                locations_id_location_seq.NEXTVAL,
                p_name,
                p_availability_start,
                p_availability_end,
                p_id_city,
                p_id_organizer
            );
            
            DBMS_OUTPUT.PUT_LINE('Nová lokace vytvořena');
            
        ELSE
            DBMS_OUTPUT.PUT_LINE('ID: ' || p_id_location || ' - UPDATE');
            
            BEGIN
                SELECT availability_start, availability_end
                INTO v_old_start, v_old_end
                FROM locations
                WHERE id_location = p_id_location;
                
                -- Pokud se změnil čas, zrušíme neplatné žádosti
                IF TO_CHAR(v_old_start, 'HH24:MI') != TO_CHAR(p_availability_start, 'HH24:MI') OR
                   TO_CHAR(v_old_end, 'HH24:MI') != TO_CHAR(p_availability_end, 'HH24:MI') THEN
                    
                    DBMS_OUTPUT.PUT_LINE('Změna času detekována!');
                    cancel_invalid_requests(p_id_location, p_availability_start, p_availability_end);
                ELSE
                    DBMS_OUTPUT.PUT_LINE('Čas se nezměnil');
                END IF;
                
            EXCEPTION
                WHEN NO_DATA_FOUND THEN
                    RAISE_APPLICATION_ERROR(-20404, 'Lokace neexistuje');
            END;
            
            UPDATE locations SET
                "name" = p_name,
                availability_start = p_availability_start,
                availability_end = p_availability_end,
                id_city = p_id_city,
                id_organizer = p_id_organizer
            WHERE id_location = p_id_location;
            
            IF SQL%ROWCOUNT = 0 THEN
                RAISE_APPLICATION_ERROR(-20404, 'Lokace neexistuje');
            END IF;
            
            DBMS_OUTPUT.PUT_LINE('Lokace aktualizována');
        END IF;
        
        COMMIT;
        DBMS_OUTPUT.PUT_LINE('=== PERSIST_LOCATION DONE ===');
        
    EXCEPTION
        WHEN OTHERS THEN
            g_cancel_in_progress := FALSE;
            DBMS_OUTPUT.PUT_LINE('CHYBA: ' || SQLERRM);
            ROLLBACK;
            RAISE;
    END persist_location;

    PROCEDURE delete_location(
        p_id_location IN NUMBER
    ) IS
    BEGIN
        DBMS_OUTPUT.PUT_LINE('=== DELETE_LOCATION START ===');
        
        DELETE FROM locations WHERE id_location = p_id_location;
        
        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20404, 'Lokace neexistuje');
        END IF;
        
        COMMIT;
        DBMS_OUTPUT.PUT_LINE('Lokace smazána');
        
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('CHYBA: ' || SQLERRM);
            ROLLBACK;
            RAISE;
    END delete_location;

END locations_pkg;
/