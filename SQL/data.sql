-- Countries
INSERT INTO countries ("name") VALUES ('Czech Republic');
INSERT INTO countries ("name") VALUES ('Slovakia');
INSERT INTO countries ("name") VALUES ('Germany');

-- Cities
INSERT INTO cities ("name", id_country) VALUES ('Prague', 1);
INSERT INTO cities ("name", id_country) VALUES ('Bratislava', 2);
INSERT INTO cities ("name", id_country) VALUES ('Berlin', 3);

-- Notification Types
INSERT INTO notification_types ("code", "name", "description") VALUES ('ALLOC_SUCCESS', 'Alokace úspěšná', 'Vaše žádost o rezervaci místnosti byla úspěšně alokována.');
INSERT INTO notification_types ("code", "name", "description") VALUES ('ALLOC_FAIL', 'Alokace neúspěšná', 'Vaše žádost o rezervaci místnosti nebyla alokována.');

-- Roles (v pořadí podle úrovně oprávnění)
INSERT INTO roles ("name") VALUES ('Administrator');        -- ID: 1 - nejvyšší práva
INSERT INTO roles ("name") VALUES ('Manager');     -- ID: 2 - správce budov/místností
INSERT INTO roles ("name") VALUES ('User');                 -- ID: 3 - běžný uživatel
INSERT INTO roles ("name") VALUES ('Guest');                -- ID: 4 - nejnižší práva (jen čtení)

-- Organizers - OPRAVENO: správné přiřazení rolí
DECLARE
    v_organizer_id NUMBER;
BEGIN
    -- Administrator: John Doe
    user_management_pkg.register_user(
        p_name => 'John Doe',
        p_email => 'john.doe@example.com',
        p_password => 'admin123',
        p_id_role => 1,  -- Administrator
        o_organizer_id => v_organizer_id
    );
    DBMS_OUTPUT.PUT_LINE('John Doe ID: ' || v_organizer_id);
    
    -- Manager: Jane Smith (OPRAVENO z User)
    user_management_pkg.register_user(
        p_name => 'Jane Smith',
        p_email => 'jane.smith@example.com',
        p_password => 'manager123',
        p_id_role => 2,  -- Manager
        o_organizer_id => v_organizer_id
    );
    DBMS_OUTPUT.PUT_LINE('Jane Smith ID: ' || v_organizer_id);
    
    -- Guest: Peter Parker (OPRAVENO z User)
    user_management_pkg.register_user(
        p_name => 'Peter Parker',
        p_email => 'peter.parker@example.com',
        p_password => 'guest123',
        p_id_role => 4,  -- Guest
        o_organizer_id => v_organizer_id
    );
    DBMS_OUTPUT.PUT_LINE('Peter Parker ID: ' || v_organizer_id);
END;
/

-- Nastavení náhradníků
BEGIN
    -- Jane Smith má náhradníka John Doe (ID=1)
    user_management_pkg.set_organizer_substitute(
        p_id_organizer => 2,
        p_id_organizer_substitute => 1
    );
    
    -- Peter Parker má náhradníka Jane Smith (ID=2)
    user_management_pkg.set_organizer_substitute(
        p_id_organizer => 3,
        p_id_organizer_substitute => 2
    );
END;
/

-- Locations
INSERT INTO locations ("name", availability_start, availability_end, id_city, id_organizer) 
VALUES ('Main Hall', TO_DATE('2024-12-06 08:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-06 17:00', 'YYYY-MM-DD HH24:MI'), 1, 1);
INSERT INTO locations ("name", availability_start, availability_end, id_city, id_organizer) 
VALUES ('Conference Room A', TO_DATE('2024-12-06 09:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-06 18:00', 'YYYY-MM-DD HH24:MI'), 2, 2);
INSERT INTO locations ("name", availability_start, availability_end, id_city, id_organizer) 
VALUES ('Berlin Office', TO_DATE('2024-12-06 10:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-06 19:00', 'YYYY-MM-DD HH24:MI'), 3, 3);

-- Rooms
INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Room 101', 50, 'MEETING_ROOM', 1, 1);
INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Room 202', 30, 'PRESENTATION_ROOM', 2, 2);
INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Room 303', 20, 'MEETING_ROOM', 3, 3);

-- Meeting Rooms
INSERT INTO meeting_rooms (id_room, vc_ready) 
SELECT id_room, 'Y' FROM rooms WHERE "name" = 'Room 101';
INSERT INTO meeting_rooms (id_room, vc_ready) 
SELECT id_room, 'N' FROM rooms WHERE "name" = 'Room 303';

-- Presentation Rooms
INSERT INTO presentation_rooms (id_room, podium_size) 
SELECT id_room, 10 FROM rooms WHERE "name" = 'Room 202';

-- MEETING_RREQUEST #1: 2 hodiny (09:00 - 11:00)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 20, 'MEETING_RREQUEST',
        TO_DATE('2024-12-07 09:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-07 11:00', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '2' HOUR,
        1, 1);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'Y');

-- PRESENTATION_RREQUEST #1: 2 hodiny (10:00 - 12:00)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 15, 'PRESENTATION_RREQUEST',
        TO_DATE('2024-12-08 10:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-08 12:00', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '2' HOUR,
        2, 2);
INSERT INTO presentation_rrequests (id_room_request, podium_size)
VALUES (room_requests_id_room_request.CURRVAL, 10);

-- MEETING_RREQUEST #2: 3 hodiny 30 minut (13:00 - 16:30)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 25, 'MEETING_RREQUEST',
        TO_DATE('2024-12-09 13:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-09 16:30', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '3:30' HOUR TO MINUTE,
        1, 1);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'N');

-- PRESENTATION_RREQUEST #2: 1 hodina 15 minut (14:00 - 15:15)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 20, 'PRESENTATION_RREQUEST',
        TO_DATE('2024-12-10 14:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-10 15:15', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '1:15' HOUR TO MINUTE,
        2, 2);
INSERT INTO presentation_rrequests (id_room_request, podium_size)
VALUES (room_requests_id_room_request.CURRVAL, 15);

-- MEETING_RREQUEST #3: Celý den (8 hodin)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 30, 'MEETING_RREQUEST',
        TO_DATE('2024-12-11 09:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-11 17:00', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '8' HOUR,
        3, 3);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'Y');

-- Images
INSERT INTO images ("data", file_name, file_suffix, created_at, id_organizer, id_location, id_room) 
VALUES (EMPTY_BLOB(), 'room_101_default', 'jpg', SYSDATE, 1, 1, 1);

INSERT INTO images ("data", file_name, file_suffix, created_at, id_organizer, id_location, id_room) 
VALUES (EMPTY_BLOB(), 'room_202_default', 'png', SYSDATE, 2, 2, 2);

INSERT INTO images ("data", file_name, file_suffix, created_at, id_organizer, id_location, id_room) 
VALUES (EMPTY_BLOB(), 'room_303_default', 'jpg', SYSDATE, 3, 3, 3);

-- ==============================================
-- PŘIDÁNÍ TESTOVACÍCH DAT PRO STRÁNKOVÁNÍ
-- ==============================================

-- 1. PŘIDÁNÍ DVOU NOVÝCH UŽIVATELŮ
DECLARE
    v_organizer_id NUMBER;
BEGIN
    -- Uživatel 4: User (běžný uživatel) - Marie Novotná
    user_management_pkg.register_user(
        p_name => 'Marie Novotná',
        p_email => 'marie.novotna@example.com',
        p_password => 'user456',
        p_id_role => 3,  -- User (běžný uživatel)
        o_organizer_id => v_organizer_id
    );
    DBMS_OUTPUT.PUT_LINE('Marie Novotná ID: ' || v_organizer_id);
    
    -- Uživatel 5: User (běžný uživatel) - Tomáš Svoboda
    user_management_pkg.register_user(
        p_name => 'Tomáš Svoboda',
        p_email => 'tomas.svoboda@example.com',
        p_password => 'user789',
        p_id_role => 3,  -- User (běžný uživatel)
        o_organizer_id => v_organizer_id
    );
    DBMS_OUTPUT.PUT_LINE('Tomáš Svoboda ID: ' || v_organizer_id);
END;
/

-- Nastavení náhradníků pro nové uživatele
BEGIN
    -- Marie Novotná má náhradníka Jane Smith (Manager, ID=2)
    user_management_pkg.set_organizer_substitute(
        p_id_organizer => 4,
        p_id_organizer_substitute => 2
    );
    
    -- Tomáš Svoboda má náhradníka Marie Novotnou (User, ID=4)
    user_management_pkg.set_organizer_substitute(
        p_id_organizer => 5,
        p_id_organizer_substitute => 4
    );
END;
/

COMMIT;

-- ==============================================
-- 2. ŽÁDOSTI PRO MARII NOVOTNOU (ID: 4)
-- ==============================================

-- Žádost #1: MEETING REQUEST - úspěšná (Main Hall, Prague)
-- 12.12.2024, 09:00-11:00 (2 hodiny)
INSERT INTO room_requests (
    id_room_request, min_capacity, "type", 
    reservation_start, reservation_end, reservation_length, 
    id_location, id_organizer
) VALUES (
    room_requests_id_room_request.NEXTVAL, 
    25, 
    'MEETING_RREQUEST',
    TO_DATE('2024-12-12 09:00', 'YYYY-MM-DD HH24:MI'),
    TO_DATE('2024-12-12 11:00', 'YYYY-MM-DD HH24:MI'),
    INTERVAL '2' HOUR,
    1,  -- Main Hall (Prague)
    4   -- Marie Novotná
);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'Y');

-- Žádost #2: PRESENTATION REQUEST - úspěšná (Conference Room A, Bratislava)
-- 13.12.2024, 14:00-16:30 (2.5 hodiny)
INSERT INTO room_requests (
    id_room_request, min_capacity, "type", 
    reservation_start, reservation_end, reservation_length, 
    id_location, id_organizer
) VALUES (
    room_requests_id_room_request.NEXTVAL, 
    20, 
    'PRESENTATION_RREQUEST',
    TO_DATE('2024-12-13 14:00', 'YYYY-MM-DD HH24:MI'),
    TO_DATE('2024-12-13 16:30', 'YYYY-MM-DD HH24:MI'),
    INTERVAL '2:30' HOUR TO MINUTE,
    2,  -- Conference Room A (Bratislava)
    4   -- Marie Novotná
);
INSERT INTO presentation_rrequests (id_room_request, podium_size)
VALUES (room_requests_id_room_request.CURRVAL, 10);

-- ==============================================
-- 3. ŽÁDOSTI PRO TOMÁŠE SVOBODU (ID: 5)
-- ==============================================

-- Žádost #3: MEETING REQUEST - úspěšná (Berlin Office)
-- 14.12.2024, 10:00-12:00 (2 hodiny)
INSERT INTO room_requests (
    id_room_request, min_capacity, "type", 
    reservation_start, reservation_end, reservation_length, 
    id_location, id_organizer
) VALUES (
    room_requests_id_room_request.NEXTVAL, 
    15, 
    'MEETING_RREQUEST',
    TO_DATE('2024-12-14 10:00', 'YYYY-MM-DD HH24:MI'),
    TO_DATE('2024-12-14 12:00', 'YYYY-MM-DD HH24:MI'),
    INTERVAL '2' HOUR,
    3,  -- Berlin Office
    5   -- Tomáš Svoboda
);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'N');

-- Žádost #4: PRESENTATION REQUEST - neúspěšná (Conference Room A)
-- 15.12.2024, 09:00-10:30 (1.5 hodiny)
--nedostatek kapacity - confere
INSERT INTO room_requests (
    id_room_request, min_capacity, "type", 
    reservation_start, reservation_end, reservation_length, 
    id_location, id_organizer
) VALUES (
    room_requests_id_room_request.NEXTVAL, 
    18, 
    'PRESENTATION_RREQUEST',
    TO_DATE('2024-12-15 09:00', 'YYYY-MM-DD HH24:MI'),
    TO_DATE('2024-12-15 10:30', 'YYYY-MM-DD HH24:MI'),
    INTERVAL '1:30' HOUR TO MINUTE,
    2,  -- Conference Room A (Bratislava)
    5   -- Tomáš Svoboda
);
INSERT INTO presentation_rrequests (id_room_request, podium_size)
VALUES (room_requests_id_room_request.CURRVAL, 15); 

COMMIT;

-- ==============================================
-- 4. KONFLIKTNÍ ŽÁDOST - ZAČÍNÁ 1H PŘED KONCEM
-- ==============================================
-- Existující rezervace Marie Novotné: 12.12.2024 09:00-11:00 (Room 101)
-- Konfliktní žádost Tomáše: 12.12.2024 10:00-12:00 (překryv 10:00-11:00)

DECLARE
    v_request_id NUMBER;
BEGIN
    DBMS_OUTPUT.PUT_LINE('=== VYTVÁŘENÍ KONFLIKTNÍ ŽÁDOSTI ===');
    
    -- Vložení základního room_request
    INSERT INTO room_requests (
        id_room_request, min_capacity, "type", 
        reservation_start, reservation_end, reservation_length, 
        id_location, id_organizer
    ) VALUES (
        room_requests_id_room_request.NEXTVAL, 
        30, 
        'MEETING_RREQUEST',
        TO_DATE('2024-12-12 10:00', 'YYYY-MM-DD HH24:MI'),  -- Začíná hodinu před koncem existující
        TO_DATE('2024-12-12 12:00', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '2' HOUR,
        1,  -- Main Hall (Praha) - stejná lokace jako Marie
        5   -- Tomáš Svoboda
    ) RETURNING id_room_request INTO v_request_id;
    
    DBMS_OUTPUT.PUT_LINE('Konfliktní žádost ID: ' || v_request_id);
    
    -- Vložení meeting_rrequest (trigger se pokusí alokovat, ale selže)
    INSERT INTO meeting_rrequests (id_room_request, vc_ready)
    VALUES (v_request_id, 'Y');
    
    DBMS_OUTPUT.PUT_LINE('✓ Konfliktní žádost vytvořena - očekává se notifikace ALLOC_FAIL');
    
EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('CHYBA: ' || SQLERRM);
        ROLLBACK;
        RAISE;
END;
/

COMMIT;

-- ==============================================
-- 5. DALŠÍ ŽÁDOSTI PRO TESTOVÁNÍ STRÁNKOVÁNÍ
-- ==============================================

-- Žádost #5: Marie - Meeting (úspěch)
INSERT INTO room_requests (
    id_room_request, min_capacity, "type", 
    reservation_start, reservation_end, reservation_length, 
    id_location, id_organizer
) VALUES (
    room_requests_id_room_request.NEXTVAL, 
    12, 
    'MEETING_RREQUEST',
    TO_DATE('2024-12-16 13:00', 'YYYY-MM-DD HH24:MI'),
    TO_DATE('2024-12-16 14:30', 'YYYY-MM-DD HH24:MI'),
    INTERVAL '1:30' HOUR TO MINUTE,
    3,  -- Berlin Office
    4   -- Marie Novotná
);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'N');

-- Žádost #6: Tomáš - Presentation (úspěch)
INSERT INTO room_requests (
    id_room_request, min_capacity, "type", 
    reservation_start, reservation_end, reservation_length, 
    id_location, id_organizer
) VALUES (
    room_requests_id_room_request.NEXTVAL, 
    22, 
    'PRESENTATION_RREQUEST',
    TO_DATE('2024-12-17 11:00', 'YYYY-MM-DD HH24:MI'),
    TO_DATE('2024-12-17 13:00', 'YYYY-MM-DD HH24:MI'),
    INTERVAL '2' HOUR,
    2,  -- Conference Room A
    5   -- Tomáš Svoboda
);
INSERT INTO presentation_rrequests (id_room_request, podium_size)
VALUES (room_requests_id_room_request.CURRVAL, 10);

-- Žádost #7: Marie - Meeting (úspěch)
INSERT INTO room_requests (
    id_room_request, min_capacity, "type", 
    reservation_start, reservation_end, reservation_length, 
    id_location, id_organizer
) VALUES (
    room_requests_id_room_request.NEXTVAL, 
    40, 
    'MEETING_RREQUEST',
    TO_DATE('2024-12-18 09:30', 'YYYY-MM-DD HH24:MI'),
    TO_DATE('2024-12-18 12:00', 'YYYY-MM-DD HH24:MI'),
    INTERVAL '2:30' HOUR TO MINUTE,
    1,  -- Main Hall
    4   -- Marie Novotná
);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'Y');

COMMIT;

-- ==============================================
-- SOUHRN PŘIDANÝCH DAT
-- ==============================================
SELECT 
    'PŘEHLED NOVÝCH DAT' AS INFO,
    (SELECT COUNT(*) FROM organizers WHERE id_organizer IN (4, 5)) AS "Noví uživatelé",
    (SELECT COUNT(*) FROM room_requests WHERE id_organizer IN (4, 5)) AS "Nové žádosti",
    (SELECT COUNT(*) FROM notifications WHERE id_organizer IN (4, 5)) AS "Nové notifikace"
FROM DUAL;

-- Výpis nových uživatelů s rolemi
SELECT 
    o.id_organizer,
    o."name",
    o.email,
    r."name" AS role_name,
    (SELECT "name" FROM organizers WHERE id_organizer = o.id_organizer_substitute) AS substitute_name
FROM organizers o
JOIN roles r ON o.id_role = r.id_role
WHERE o.id_organizer IN (4, 5)
ORDER BY o.id_organizer;

-- Výpis všech žádostí nových uživatelů
SELECT 
    rr.id_room_request,
    o."name" AS organizer_name,
    rr."type",
    TO_CHAR(rr.reservation_start, 'DD.MM.YYYY HH24:MI') AS start_time,
    TO_CHAR(rr.reservation_end, 'DD.MM.YYYY HH24:MI') AS end_time,
    l."name" AS location_name
FROM room_requests rr
JOIN organizers o ON rr.id_organizer = o.id_organizer
JOIN locations l ON rr.id_location = l.id_location
WHERE rr.id_organizer IN (4, 5)
ORDER BY rr.reservation_start;

-- Kontrola konfliktní žádosti
SELECT 
    n.id_notification,
    nt."name" AS notification_type,
    nt."code" AS notification_code,
    n.delivered AS delivered_flag,
    o."name" AS organizer_name,
    TO_CHAR(rr.reservation_start, 'DD.MM.YYYY HH24:MI') AS request_start,
    TO_CHAR(rr.reservation_end, 'DD.MM.YYYY HH24:MI') AS request_end
FROM notifications n
JOIN notification_types nt ON n.id_notification_type = nt.id_notification_type
JOIN organizers o ON n.id_organizer = o.id_organizer
JOIN room_requests rr ON n.id_room_request = rr.id_room_request
WHERE rr.id_organizer IN (4, 5)
ORDER BY n.id_notification DESC;


COMMIT;