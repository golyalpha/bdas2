-- =========================================================
-- TEST: Smazání blokující rezervace → automatická realokace
-- =========================================================

SET SERVEROUTPUT ON;

-- ==============================================
-- KROK 1: ZJIŠTĚNÍ AKTUÁLNÍHO STAVU
-- ==============================================
PROMPT === STAV PŘED SMAZÁNÍM ===

-- Zobraz existující rezervace v Main Hall (location_id = 1)
PROMPT
PROMPT 🏢 Rezervace v Main Hall (Praha):
SELECT 
    r.id_reservation,
    TO_CHAR(r."start", 'DD.MM.YYYY HH24:MI') AS start_time,
    TO_CHAR(r."end", 'DD.MM.YYYY HH24:MI') AS end_time,
    rm.id_room,
    rm."name" AS room_name,
    o."name" AS organizer_name
FROM reservations r
JOIN rooms rm ON r.id_room = rm.id_room
JOIN organizers o ON r.id_organizer = o.id_organizer
WHERE rm.id_location = 1
ORDER BY r."start";

-- Zobraz nealokované requesty v Main Hall
PROMPT
PROMPT ⏳ Nealokované žádosti v Main Hall:
SELECT 
    rr.id_room_request,
    TO_CHAR(rr.reservation_start, 'DD.MM.YYYY HH24:MI') AS start_time,
    TO_CHAR(rr.reservation_end, 'DD.MM.YYYY HH24:MI') AS end_time,
    rr.min_capacity,
    o."name" AS organizer_name
FROM room_requests rr
JOIN organizers o ON rr.id_organizer = o.id_organizer
WHERE rr.id_location = 1
AND NOT EXISTS (
    SELECT 1 FROM reservations res WHERE res.id_room_request = rr.id_room_request
)
ORDER BY rr.reservation_start;

-- ==============================================
-- KROK 2: SMAZÁNÍ BLOKUJÍCÍ REZERVACE
-- ==============================================
PROMPT
PROMPT === MAZÁNÍ REZERVACE ===
PROMPT

--rezervaci s id 4
BEGIN
    reservations_pkg.delete_reservation(4);
    COMMIT;
END;
/

-- ==============================================
-- KROK 3: KONTROLA VÝSLEDKU
-- ==============================================
PROMPT
PROMPT === STAV PO SMAZÁNÍ ===

-- Zobraz aktuální rezervace v Main Hall
PROMPT
PROMPT 🏢 Rezervace v Main Hall po realokaci:
SELECT 
    r.id_reservation,
    TO_CHAR(r."start", 'DD.MM.YYYY HH24:MI') AS start_time,
    TO_CHAR(r."end", 'DD.MM.YYYY HH24:MI') AS end_time,
    rm."name" AS room_name,
    o."name" AS organizer_name
FROM reservations r
JOIN rooms rm ON r.id_room = rm.id_room
JOIN organizers o ON r.id_organizer = o.id_organizer
WHERE rm.id_location = 1
ORDER BY r."start";

-- Zobraz nealokované requesty (mělo by být méně než předtím)
PROMPT
PROMPT ⏳ Zbývající nealokované žádosti v Main Hall:
SELECT 
    rr.id_room_request,
    TO_CHAR(rr.reservation_start, 'DD.MM.YYYY HH24:MI') AS start_time,
    TO_CHAR(rr.reservation_end, 'DD.MM.YYYY HH24:MI') AS end_time,
    rr.min_capacity,
    o."name" AS organizer_name
FROM room_requests rr
JOIN organizers o ON rr.id_organizer = o.id_organizer
WHERE rr.id_location = 1
AND NOT EXISTS (
    SELECT 1 FROM reservations res WHERE res.id_room_request = rr.id_room_request
)
ORDER BY rr.reservation_start;

-- Zobraz nové notifikace
PROMPT
PROMPT 🔔 Nové notifikace po realokaci:
SELECT 
    n.id_notification,
    nt."code" AS notification_code,
    nt."name" AS notification_name,
    o."name" AS organizer_name,
    TO_CHAR(rr.reservation_start, 'DD.MM.YYYY HH24:MI') AS request_start
FROM notifications n
JOIN notification_types nt ON n.id_notification_type = nt.id_notification_type
JOIN organizers o ON n.id_organizer = o.id_organizer
JOIN room_requests rr ON n.id_room_request = rr.id_room_request
WHERE rr.id_location = 1
ORDER BY n.id_notification DESC
FETCH FIRST 5 ROWS ONLY;

PROMPT
PROMPT === TEST DOKONČEN ===