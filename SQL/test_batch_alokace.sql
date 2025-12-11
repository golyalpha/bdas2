-- =========================================================================
-- TEST: Velká rezervace blokuje 5 menších hodinových requestů
-- Po smazání velké rezervace → všech 5 menších se alokuje
-- =========================================================================

SET SERVEROUTPUT ON;

PROMPT ========================================
PROMPT    FÁZE 1: PŘÍPRAVA TESTOVACÍHO PROSTŘEDÍ
PROMPT ========================================

-- ==============================================
-- KROK 1: Vytvoření velké blokující rezervace
-- ==============================================
PROMPT
PROMPT === VYTVÁŘENÍ VELKÉ BLOKUJÍCÍ REZERVACE ===
PROMPT Časový slot: 20.12.2024 09:00-14:00 (5 hodin)
PROMPT Místnost: Room 101 (Main Hall, Praha)
PROMPT Organizátor: John Doe (ID: 1)

DECLARE
    v_request_id NUMBER;
    v_reservation_id NUMBER;
BEGIN
    -- Vložení room_request pro velkou rezervaci
    INSERT INTO room_requests (
        id_room_request, min_capacity, "type", 
        reservation_start, reservation_end, reservation_length, 
        id_location, id_organizer
    ) VALUES (
        room_requests_id_room_request.NEXTVAL, 
        30, 
        'MEETING_RREQUEST',
        TO_DATE('2024-12-20 09:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-20 14:00', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '5' HOUR,
        1,  -- Main Hall (Praha)
        1   -- John Doe
    ) RETURNING id_room_request INTO v_request_id;
    
    DBMS_OUTPUT.PUT_LINE('✅ Vytvořen room_request ID: ' || v_request_id);
    
    -- Vložení meeting_rrequest (trigger automaticky alokuje)
    INSERT INTO meeting_rrequests (id_room_request, vc_ready)
    VALUES (v_request_id, 'Y');
    
    -- Ověření vytvoření rezervace
    SELECT id_reservation INTO v_reservation_id
    FROM reservations
    WHERE id_room_request = v_request_id;
    
    DBMS_OUTPUT.PUT_LINE('✅ Vytvořena velká rezervace ID: ' || v_reservation_id);
    DBMS_OUTPUT.PUT_LINE('   📅 Čas: 20.12.2024 09:00-14:00 (5 hodin)');
    DBMS_OUTPUT.PUT_LINE('   🏢 Místnost: Room 101');
    
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('❌ CHYBA: Rezervace nebyla vytvořena!');
        ROLLBACK;
        RAISE;
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('❌ CHYBA: ' || SQLERRM);
        ROLLBACK;
        RAISE;
END;
/

COMMIT;

-- ==============================================
-- KROK 2: Vytvoření 5 menších hodinových requestů
-- ==============================================
PROMPT
PROMPT === VYTVÁŘENÍ 5 MENŠÍCH HODINOVÝCH REQUESTŮ ===
PROMPT (Všechny budou blokované velkou rezervací)

DECLARE
    v_request_id NUMBER;
    v_start_time DATE;
    v_end_time DATE;
BEGIN
    -- REQUEST #1: 09:00-10:00 (Marie Novotná)
    DBMS_OUTPUT.PUT_LINE(CHR(10) || '📋 Request #1: 09:00-10:00 (Marie Novotná)');
    v_start_time := TO_DATE('2024-12-20 09:00', 'YYYY-MM-DD HH24:MI');
    v_end_time := TO_DATE('2024-12-20 10:00', 'YYYY-MM-DD HH24:MI');
    
    INSERT INTO room_requests (
        id_room_request, min_capacity, "type", 
        reservation_start, reservation_end, reservation_length, 
        id_location, id_organizer
    ) VALUES (
        room_requests_id_room_request.NEXTVAL, 
        15, 
        'MEETING_RREQUEST',
        v_start_time, v_end_time,
        INTERVAL '1' HOUR,
        1, 4  -- Marie Novotná
    ) RETURNING id_room_request INTO v_request_id;
    
    INSERT INTO meeting_rrequests (id_room_request, vc_ready)
    VALUES (v_request_id, 'N');
    
    DBMS_OUTPUT.PUT_LINE('   ✅ Request ID: ' || v_request_id);

    -- REQUEST #2: 10:00-11:00 (Tomáš Svoboda)
    DBMS_OUTPUT.PUT_LINE(CHR(10) || '📋 Request #2: 10:00-11:00 (Tomáš Svoboda)');
    v_start_time := TO_DATE('2024-12-20 10:00', 'YYYY-MM-DD HH24:MI');
    v_end_time := TO_DATE('2024-12-20 11:00', 'YYYY-MM-DD HH24:MI');
    
    INSERT INTO room_requests (
        id_room_request, min_capacity, "type", 
        reservation_start, reservation_end, reservation_length, 
        id_location, id_organizer
    ) VALUES (
        room_requests_id_room_request.NEXTVAL, 
        20, 
        'MEETING_RREQUEST',
        v_start_time, v_end_time,
        INTERVAL '1' HOUR,
        1, 5  -- Tomáš Svoboda
    ) RETURNING id_room_request INTO v_request_id;
    
    INSERT INTO meeting_rrequests (id_room_request, vc_ready)
    VALUES (v_request_id, 'Y');
    
    DBMS_OUTPUT.PUT_LINE('   ✅ Request ID: ' || v_request_id);

    -- REQUEST #3: 11:00-12:00 (Marie Novotná)
    DBMS_OUTPUT.PUT_LINE(CHR(10) || '📋 Request #3: 11:00-12:00 (Marie Novotná)');
    v_start_time := TO_DATE('2024-12-20 11:00', 'YYYY-MM-DD HH24:MI');
    v_end_time := TO_DATE('2024-12-20 12:00', 'YYYY-MM-DD HH24:MI');
    
    INSERT INTO room_requests (
        id_room_request, min_capacity, "type", 
        reservation_start, reservation_end, reservation_length, 
        id_location, id_organizer
    ) VALUES (
        room_requests_id_room_request.NEXTVAL, 
        18, 
        'MEETING_RREQUEST',
        v_start_time, v_end_time,
        INTERVAL '1' HOUR,
        1, 4  -- Marie Novotná
    ) RETURNING id_room_request INTO v_request_id;
    
    INSERT INTO meeting_rrequests (id_room_request, vc_ready)
    VALUES (v_request_id, 'N');
    
    DBMS_OUTPUT.PUT_LINE('   ✅ Request ID: ' || v_request_id);

    -- REQUEST #4: 12:00-13:00 (Tomáš Svoboda)
    DBMS_OUTPUT.PUT_LINE(CHR(10) || '📋 Request #4: 12:00-13:00 (Tomáš Svoboda)');
    v_start_time := TO_DATE('2024-12-20 12:00', 'YYYY-MM-DD HH24:MI');
    v_end_time := TO_DATE('2024-12-20 13:00', 'YYYY-MM-DD HH24:MI');
    
    INSERT INTO room_requests (
        id_room_request, min_capacity, "type", 
        reservation_start, reservation_end, reservation_length, 
        id_location, id_organizer
    ) VALUES (
        room_requests_id_room_request.NEXTVAL, 
        22, 
        'MEETING_RREQUEST',
        v_start_time, v_end_time,
        INTERVAL '1' HOUR,
        1, 5  -- Tomáš Svoboda
    ) RETURNING id_room_request INTO v_request_id;
    
    INSERT INTO meeting_rrequests (id_room_request, vc_ready)
    VALUES (v_request_id, 'Y');
    
    DBMS_OUTPUT.PUT_LINE('   ✅ Request ID: ' || v_request_id);

    -- REQUEST #5: 13:00-14:00 (Marie Novotná)
    DBMS_OUTPUT.PUT_LINE(CHR(10) || '📋 Request #5: 13:00-14:00 (Marie Novotná)');
    v_start_time := TO_DATE('2024-12-20 13:00', 'YYYY-MM-DD HH24:MI');
    v_end_time := TO_DATE('2024-12-20 14:00', 'YYYY-MM-DD HH24:MI');
    
    INSERT INTO room_requests (
        id_room_request, min_capacity, "type", 
        reservation_start, reservation_end, reservation_length, 
        id_location, id_organizer
    ) VALUES (
        room_requests_id_room_request.NEXTVAL, 
        25, 
        'MEETING_RREQUEST',
        v_start_time, v_end_time,
        INTERVAL '1' HOUR,
        1, 4  -- Marie Novotná
    ) RETURNING id_room_request INTO v_request_id;
    
    INSERT INTO meeting_rrequests (id_room_request, vc_ready)
    VALUES (v_request_id, 'N');
    
    DBMS_OUTPUT.PUT_LINE('   ✅ Request ID: ' || v_request_id);
    
    DBMS_OUTPUT.PUT_LINE(CHR(10) || '✅ Všech 5 requestů vytvořeno');
    
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('❌ CHYBA: ' || SQLERRM);
        ROLLBACK;
        RAISE;
END;
/

COMMIT;

-- ==============================================
-- KROK 3: ZOBRAZENÍ STAVU PŘED SMAZÁNÍM
-- ==============================================
PROMPT
PROMPT ========================================
PROMPT    FÁZE 2: STAV PŘED SMAZÁNÍM VELKÉ REZERVACE
PROMPT ========================================

PROMPT
PROMPT 🏢 Rezervace v Main Hall (20.12.2024):
SELECT 
    r.id_reservation,
    TO_CHAR(r."start", 'DD.MM.YYYY HH24:MI') AS start_time,
    TO_CHAR(r."end", 'DD.MM.YYYY HH24:MI') AS end_time,
    ROUND((r."end" - r."start") * 24, 2) AS hours,
    o."name" AS organizer_name
FROM reservations r
JOIN rooms rm ON r.id_room = rm.id_room
JOIN organizers o ON r.id_organizer = o.id_organizer
WHERE rm.id_location = 1
  AND r."start" >= TO_DATE('2024-12-20 00:00', 'YYYY-MM-DD HH24:MI')
  AND r."start" < TO_DATE('2024-12-21 00:00', 'YYYY-MM-DD HH24:MI')
ORDER BY r."start";

PROMPT
PROMPT ⏳ Nealokované žádosti v Main Hall (20.12.2024):
SELECT 
    rr.id_room_request,
    TO_CHAR(rr.reservation_start, 'DD.MM.YYYY HH24:MI') AS start_time,
    TO_CHAR(rr.reservation_end, 'DD.MM.YYYY HH24:MI') AS end_time,
    rr.min_capacity,
    o."name" AS organizer_name
FROM room_requests rr
JOIN organizers o ON rr.id_organizer = o.id_organizer
WHERE rr.id_location = 1
  AND rr.reservation_start >= TO_DATE('2024-12-20 00:00', 'YYYY-MM-DD HH24:MI')
  AND rr.reservation_start < TO_DATE('2024-12-21 00:00', 'YYYY-MM-DD HH24:MI')
  AND NOT EXISTS (
      SELECT 1 FROM reservations res WHERE res.id_room_request = rr.id_room_request
  )
ORDER BY rr.reservation_start;

PROMPT
PROMPT 🔔 Notifikace FAIL (blokované requesty):
SELECT 
    n.id_notification,
    nt."code" AS code,
    o."name" AS organizer_name,
    TO_CHAR(rr.reservation_start, 'HH24:MI') || '-' || TO_CHAR(rr.reservation_end, 'HH24:MI') AS time_slot
FROM notifications n
JOIN notification_types nt ON n.id_notification_type = nt.id_notification_type
JOIN organizers o ON n.id_organizer = o.id_organizer
JOIN room_requests rr ON n.id_room_request = rr.id_room_request
WHERE rr.id_location = 1
  AND rr.reservation_start >= TO_DATE('2024-12-20 00:00', 'YYYY-MM-DD HH24:MI')
  AND rr.reservation_start < TO_DATE('2024-12-21 00:00', 'YYYY-MM-DD HH24:MI')
  AND nt."code" = 'ALLOC_FAIL'
ORDER BY rr.reservation_start;

-- ==============================================
-- KROK 4: SMAZÁNÍ VELKÉ REZERVACE
-- ==============================================
PROMPT
PROMPT ========================================
PROMPT    FÁZE 3: MAZÁNÍ VELKÉ REZERVACE
PROMPT ========================================

DECLARE
    v_id_reservation_to_delete NUMBER;
BEGIN
    -- Najít ID velké rezervace (09:00-14:00)
    SELECT id_reservation INTO v_id_reservation_to_delete
    FROM reservations r
    JOIN rooms rm ON r.id_room = rm.id_room
    WHERE rm.id_location = 1
      AND r."start" = TO_DATE('2024-12-20 09:00', 'YYYY-MM-DD HH24:MI')
      AND r."end" = TO_DATE('2024-12-20 14:00', 'YYYY-MM-DD HH24:MI');
    
    DBMS_OUTPUT.PUT_LINE(CHR(10) || '🗑️  Mažu velkou rezervaci ID: ' || v_id_reservation_to_delete);
    DBMS_OUTPUT.PUT_LINE('   📅 Čas: 20.12.2024 09:00-14:00 (5 hodin)');
    
    -- Smazání rezervace (spustí realokaci)
    reservations_pkg.delete_reservation(p_id_reservation => v_id_reservation_to_delete);
    
    DBMS_OUTPUT.PUT_LINE(CHR(10) || '✅ Velká rezervace smazána a realokace dokončena');
    
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('❌ CHYBA: Velká rezervace nenalezena!');
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('❌ CHYBA při mazání: ' || SQLERRM);
END;
/

-- ==============================================
-- KROK 5: ZOBRAZENÍ STAVU PO SMAZÁNÍ
-- ==============================================
PROMPT
PROMPT ========================================
PROMPT    FÁZE 4: STAV PO REALOKACI
PROMPT ========================================

PROMPT
PROMPT 🏢 Rezervace v Main Hall (20.12.2024) - PO REALOKACI:
SELECT 
    r.id_reservation,
    TO_CHAR(r."start", 'DD.MM.YYYY HH24:MI') AS start_time,
    TO_CHAR(r."end", 'DD.MM.YYYY HH24:MI') AS end_time,
    ROUND((r."end" - r."start") * 24, 1) AS hours,
    o."name" AS organizer_name
FROM reservations r
JOIN rooms rm ON r.id_room = rm.id_room
JOIN organizers o ON r.id_organizer = o.id_organizer
WHERE rm.id_location = 1
  AND r."start" >= TO_DATE('2024-12-20 00:00', 'YYYY-MM-DD HH24:MI')
  AND r."start" < TO_DATE('2024-12-21 00:00', 'YYYY-MM-DD HH24:MI')
ORDER BY r."start";

PROMPT
PROMPT ⏳ Zbývající nealokované žádosti v Main Hall (20.12.2024):
SELECT 
    rr.id_room_request,
    TO_CHAR(rr.reservation_start, 'HH24:MI') || '-' || TO_CHAR(rr.reservation_end, 'HH24:MI') AS time_slot,
    rr.min_capacity,
    o."name" AS organizer_name
FROM room_requests rr
JOIN organizers o ON rr.id_organizer = o.id_organizer
WHERE rr.id_location = 1
  AND rr.reservation_start >= TO_DATE('2024-12-20 00:00', 'YYYY-MM-DD HH24:MI')
  AND rr.reservation_start < TO_DATE('2024-12-21 00:00', 'YYYY-MM-DD HH24:MI')
  AND NOT EXISTS (
      SELECT 1 FROM reservations res WHERE res.id_room_request = rr.id_room_request
  )
ORDER BY rr.reservation_start;

PROMPT
PROMPT 🔔 Notifikace SUCCESS (úspěšně alokované requesty):
SELECT 
    n.id_notification,
    nt."code" AS code,
    n.delivered AS delivered,
    o."name" AS organizer_name,
    TO_CHAR(rr.reservation_start, 'HH24:MI') || '-' || TO_CHAR(rr.reservation_end, 'HH24:MI') AS time_slot
FROM notifications n
JOIN notification_types nt ON n.id_notification_type = nt.id_notification_type
JOIN organizers o ON n.id_organizer = o.id_organizer
JOIN room_requests rr ON n.id_room_request = rr.id_room_request
WHERE rr.id_location = 1
  AND rr.reservation_start >= TO_DATE('2024-12-20 00:00', 'YYYY-MM-DD HH24:MI')
  AND rr.reservation_start < TO_DATE('2024-12-21 00:00', 'YYYY-MM-DD HH24:MI')
  AND nt."code" = 'ALLOC_SUCCESS'
ORDER BY rr.reservation_start;

-- ==============================================
-- KROK 6: FINÁLNÍ STATISTIKY
-- ==============================================
PROMPT
PROMPT ========================================
PROMPT    FÁZE 5: FINÁLNÍ STATISTIKY
PROMPT ========================================

DECLARE
    v_allocated_count NUMBER;
    v_unallocated_count NUMBER;
    v_success_notifications NUMBER;
BEGIN
    -- Počet alokovaných requestů
    SELECT COUNT(*) INTO v_allocated_count
    FROM reservations r
    JOIN rooms rm ON r.id_room = rm.id_room
    WHERE rm.id_location = 1
      AND r."start" >= TO_DATE('2024-12-20 00:00', 'YYYY-MM-DD HH24:MI')
      AND r."start" < TO_DATE('2024-12-21 00:00', 'YYYY-MM-DD HH24:MI');
    
    -- Počet nealokovaných requestů
    SELECT COUNT(*) INTO v_unallocated_count
    FROM room_requests rr
    WHERE rr.id_location = 1
      AND rr.reservation_start >= TO_DATE('2024-12-20 00:00', 'YYYY-MM-DD HH24:MI')
      AND rr.reservation_start < TO_DATE('2024-12-21 00:00', 'YYYY-MM-DD HH24:MI')
      AND NOT EXISTS (
          SELECT 1 FROM reservations res WHERE res.id_room_request = rr.id_room_request
      );
    
    -- Počet SUCCESS notifikací
    SELECT COUNT(*) INTO v_success_notifications
    FROM notifications n
    JOIN notification_types nt ON n.id_notification_type = nt.id_notification_type
    JOIN room_requests rr ON n.id_room_request = rr.id_room_request
    WHERE rr.id_location = 1
      AND rr.reservation_start >= TO_DATE('2024-12-20 00:00', 'YYYY-MM-DD HH24:MI')
      AND rr.reservation_start < TO_DATE('2024-12-21 00:00', 'YYYY-MM-DD HH24:MI')
      AND nt."code" = 'ALLOC_SUCCESS';
    
    DBMS_OUTPUT.PUT_LINE(CHR(10) || '📊 VÝSLEDKY TESTU:');
    DBMS_OUTPUT.PUT_LINE('   ✅ Alokované rezervace: ' || v_allocated_count || '/5');
    DBMS_OUTPUT.PUT_LINE('   ⏳ Nealokované requesty: ' || v_unallocated_count);
    DBMS_OUTPUT.PUT_LINE('   🔔 SUCCESS notifikace: ' || v_success_notifications || '/5');
    DBMS_OUTPUT.PUT_LINE('');
    
    IF v_allocated_count = 5 AND v_unallocated_count = 0 THEN
        DBMS_OUTPUT.PUT_LINE('🎉 TEST ÚSPĚŠNÝ! Všech 5 menších requestů bylo úspěšně alokováno!');
    ELSE
        DBMS_OUTPUT.PUT_LINE('❌ TEST SELHAL! Očekáváno: 5 alokací, Skutečnost: ' || v_allocated_count);
    END IF;
END;
/

PROMPT
PROMPT ========================================
PROMPT    TEST DOKONČEN
PROMPT ========================================