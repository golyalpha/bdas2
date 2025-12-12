-- Countries
INSERT INTO countries ("name") VALUES ('Czech Republic');
INSERT INTO countries ("name") VALUES ('Slovakia');
INSERT INTO countries ("name") VALUES ('Germany');
INSERT INTO countries ("name") VALUES ('Poland');
INSERT INTO countries ("name") VALUES ('Austria');
INSERT INTO countries ("name") VALUES ('Hungary');
INSERT INTO countries ("name") VALUES ('France');

-- Cities
INSERT INTO cities ("name", id_country) VALUES ('Prague', 1);
INSERT INTO cities ("name", id_country) VALUES ('Bratislava', 2);
INSERT INTO cities ("name", id_country) VALUES ('Berlin', 3);
INSERT INTO cities ("name", id_country) VALUES ('Brno', 1);
INSERT INTO cities ("name", id_country) VALUES ('Ostrava', 1);
INSERT INTO cities ("name", id_country) VALUES ('Košice', 2);
INSERT INTO cities ("name", id_country) VALUES ('Warsaw', 4);
INSERT INTO cities ("name", id_country) VALUES ('Krakow', 4);
INSERT INTO cities ("name", id_country) VALUES ('Vienna', 5);
INSERT INTO cities ("name", id_country) VALUES ('Budapest', 6);
INSERT INTO cities ("name", id_country) VALUES ('Paris', 7);
INSERT INTO cities ("name", id_country) VALUES ('Munich', 3);

-- Notification Types
INSERT INTO notification_types ("code", "name", "description") VALUES ('ALLOC_SUCCESS', 'Alokace úspěšná', 'Vaše žádost o rezervaci místnosti byla úspěšně alokována.');
INSERT INTO notification_types ("code", "name", "description") VALUES ('ALLOC_FAIL', 'Alokace neúspěšná', 'Vaše žádost o rezervaci místnosti nebyla alokována.');

-- Roles (v pořadí podle úrovně oprávnění)
INSERT INTO roles ("name") VALUES ('Administrator');        -- ID: 1 - nejvyšší práva
INSERT INTO roles ("name") VALUES ('Manager');     -- ID: 2 - správce budov/místností
INSERT INTO roles ("name") VALUES ('User');                 -- ID: 3 - běžný uživatel
INSERT INTO roles ("name") VALUES ('Guest');                -- ID: 4 - nejnižší práva (jen čtení)

-- ==============================================
-- VYTVOŘENÍ VŠECH 12 UŽIVATELŮ NAJEDNOU
-- ==============================================
DECLARE
    v_organizer_id NUMBER;
BEGIN
    -- 1. Administrator: John Doe
    user_management_pkg.register_user(
        p_name => 'John Doe',
        p_email => 'john.doe@example.com',
        p_password => 'admin123',
        p_id_role => 1,
        o_organizer_id => v_organizer_id
    );
    DBMS_OUTPUT.PUT_LINE('John Doe ID: ' || v_organizer_id);
    
    -- 2. Manager: Jane Smith
    user_management_pkg.register_user(
        p_name => 'Jane Smith',
        p_email => 'jane.smith@example.com',
        p_password => 'manager123',
        p_id_role => 2,
        o_organizer_id => v_organizer_id
    );
    DBMS_OUTPUT.PUT_LINE('Jane Smith ID: ' || v_organizer_id);
    
    -- 3. Guest: Peter Parker
    user_management_pkg.register_user(
        p_name => 'Peter Parker',
        p_email => 'peter.parker@example.com',
        p_password => 'guest123',
        p_id_role => 4,
        o_organizer_id => v_organizer_id
    );
    DBMS_OUTPUT.PUT_LINE('Peter Parker ID: ' || v_organizer_id);

    -- 4. User: Lukáš Novák
    user_management_pkg.register_user(
        p_name => 'Lukáš Novák',
        p_email => 'lukas.novak@example.com',
        p_password => 'user999',
        p_id_role => 3,
        o_organizer_id => v_organizer_id
    );
    DBMS_OUTPUT.PUT_LINE('Lukáš Novák ID: ' || v_organizer_id);
    
    -- 5. User: Marie Novotná
    user_management_pkg.register_user(
        p_name => 'Marie Novotná',
        p_email => 'marie.novotna@example.com',
        p_password => 'user456',
        p_id_role => 3,
        o_organizer_id => v_organizer_id
    );
    DBMS_OUTPUT.PUT_LINE('Marie Novotná ID: ' || v_organizer_id);
    
    -- 6. User: Tomáš Svoboda
    user_management_pkg.register_user(
        p_name => 'Tomáš Svoboda',
        p_email => 'tomas.svoboda@example.com',
        p_password => 'user789',
        p_id_role => 3,
        o_organizer_id => v_organizer_id
    );
    DBMS_OUTPUT.PUT_LINE('Tomáš Svoboda ID: ' || v_organizer_id);
    
    -- 7. Manager: Eva Horáková
    user_management_pkg.register_user(
        p_name => 'Eva Horáková',
        p_email => 'eva.horakova@example.com',
        p_password => 'manager456',
        p_id_role => 2,
        o_organizer_id => v_organizer_id
    );
    DBMS_OUTPUT.PUT_LINE('Eva Horáková ID: ' || v_organizer_id);
    
    -- 8. User: Martin Procházka
    user_management_pkg.register_user(
        p_name => 'Martin Procházka',
        p_email => 'martin.prochazka@example.com',
        p_password => 'user111',
        p_id_role => 3,
        o_organizer_id => v_organizer_id
    );
    DBMS_OUTPUT.PUT_LINE('Martin Procházka ID: ' || v_organizer_id);
    
    -- 9. User: Lenka Dvořáková
    user_management_pkg.register_user(
        p_name => 'Lenka Dvořáková',
        p_email => 'lenka.dvorakova@example.com',
        p_password => 'user222',
        p_id_role => 3,
        o_organizer_id => v_organizer_id
    );
    DBMS_OUTPUT.PUT_LINE('Lenka Dvořáková ID: ' || v_organizer_id);
    
    -- 10. Guest: Petr Kovář
    user_management_pkg.register_user(
        p_name => 'Petr Kovář',
        p_email => 'petr.kovar@example.com',
        p_password => 'guest456',
        p_id_role => 4,
        o_organizer_id => v_organizer_id
    );
    DBMS_OUTPUT.PUT_LINE('Petr Kovář ID: ' || v_organizer_id);
    
    -- 11. User: Anna Veselá
    user_management_pkg.register_user(
        p_name => 'Anna Veselá',
        p_email => 'anna.vesela@example.com',
        p_password => 'user333',
        p_id_role => 3,
        o_organizer_id => v_organizer_id
    );
    DBMS_OUTPUT.PUT_LINE('Anna Veselá ID: ' || v_organizer_id);
    
    -- 12. Manager: Jakub Černý
    user_management_pkg.register_user(
        p_name => 'Jakub Černý',
        p_email => 'jakub.cerny@example.com',
        p_password => 'manager789',
        p_id_role => 2,
        o_organizer_id => v_organizer_id
    );
    DBMS_OUTPUT.PUT_LINE('Jakub Černý ID: ' || v_organizer_id);
END;
/

-- Nastavení náhradníků pro všechny uživatele
BEGIN
    user_management_pkg.set_organizer_substitute(p_id_organizer => 2, p_id_organizer_substitute => 1);  -- Jane → John
    user_management_pkg.set_organizer_substitute(p_id_organizer => 3, p_id_organizer_substitute => 2);  -- Peter → Jane
    user_management_pkg.set_organizer_substitute(p_id_organizer => 4, p_id_organizer_substitute => 6);  -- Lukáš → Tomáš (OPRAVENO: 6 místo 5)
    user_management_pkg.set_organizer_substitute(p_id_organizer => 5, p_id_organizer_substitute => 2);  -- Marie → Jane
    user_management_pkg.set_organizer_substitute(p_id_organizer => 6, p_id_organizer_substitute => 5);  -- Tomáš → Marie
    user_management_pkg.set_organizer_substitute(p_id_organizer => 7, p_id_organizer_substitute => 1);  -- Eva → John
    user_management_pkg.set_organizer_substitute(p_id_organizer => 8, p_id_organizer_substitute => 7);  -- Martin → Eva
    user_management_pkg.set_organizer_substitute(p_id_organizer => 9, p_id_organizer_substitute => 2);  -- Lenka → Jane
    user_management_pkg.set_organizer_substitute(p_id_organizer => 10, p_id_organizer_substitute => 3); -- Petr → Peter
    user_management_pkg.set_organizer_substitute(p_id_organizer => 11, p_id_organizer_substitute => 8); -- Anna → Martin
    user_management_pkg.set_organizer_substitute(p_id_organizer => 12, p_id_organizer_substitute => 1); -- Jakub → John
END;
/

COMMIT;

-- Locations
INSERT INTO locations ("name", availability_start, availability_end, id_city, id_organizer) 
VALUES ('Main Hall', TO_DATE('2024-12-06 08:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-06 17:00', 'YYYY-MM-DD HH24:MI'), 1, 1);

INSERT INTO locations ("name", availability_start, availability_end, id_city, id_organizer) 
VALUES ('Conference Room A', TO_DATE('2024-12-06 09:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-06 18:00', 'YYYY-MM-DD HH24:MI'), 2, 2);

INSERT INTO locations ("name", availability_start, availability_end, id_city, id_organizer) 
VALUES ('Berlin Office', TO_DATE('2024-12-06 10:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-06 19:00', 'YYYY-MM-DD HH24:MI'), 3, 3);

INSERT INTO locations ("name", availability_start, availability_end, id_city, id_organizer) 
VALUES ('Brno Tech Center', TO_DATE('2024-12-06 07:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-06 19:00', 'YYYY-MM-DD HH24:MI'), 4, 7);

INSERT INTO locations ("name", availability_start, availability_end, id_city, id_organizer) 
VALUES ('Ostrava Business Hub', TO_DATE('2024-12-06 08:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-06 20:00', 'YYYY-MM-DD HH24:MI'), 5, 7);

INSERT INTO locations ("name", availability_start, availability_end, id_city, id_organizer) 
VALUES ('Warsaw Innovation Center', TO_DATE('2024-12-06 09:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-06 18:00', 'YYYY-MM-DD HH24:MI'), 7, 12);

INSERT INTO locations ("name", availability_start, availability_end, id_city, id_organizer) 
VALUES ('Krakow Conference Hall', TO_DATE('2024-12-06 08:30', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-06 17:30', 'YYYY-MM-DD HH24:MI'), 8, 12);

INSERT INTO locations ("name", availability_start, availability_end, id_city, id_organizer) 
VALUES ('Vienna Office Park', TO_DATE('2024-12-06 07:30', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-06 19:30', 'YYYY-MM-DD HH24:MI'), 9, 2);

INSERT INTO locations ("name", availability_start, availability_end, id_city, id_organizer) 
VALUES ('Budapest Meeting Point', TO_DATE('2024-12-06 08:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-06 18:00', 'YYYY-MM-DD HH24:MI'), 10, 2);

INSERT INTO locations ("name", availability_start, availability_end, id_city, id_organizer) 
VALUES ('Paris Convention Center', TO_DATE('2024-12-06 09:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-06 20:00', 'YYYY-MM-DD HH24:MI'), 11, 1);

INSERT INTO locations ("name", availability_start, availability_end, id_city, id_organizer) 
VALUES ('Munich Tech Campus', TO_DATE('2024-12-06 08:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-06 19:00', 'YYYY-MM-DD HH24:MI'), 12, 1);

COMMIT;

-- Rooms
INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Room 101', 50, 'MEETING_ROOM', 1, 1);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Room 102', 35, 'MEETING_ROOM', 1, 1);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Room 103', 28, 'PRESENTATION_ROOM', 1, 1);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Room 202', 30, 'PRESENTATION_ROOM', 2, 2);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Room 303', 20, 'MEETING_ROOM', 3, 3);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Brno Room A', 40, 'MEETING_ROOM', 4, 7);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Brno Room B', 25, 'PRESENTATION_ROOM', 4, 7);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Ostrava Hall 1', 60, 'MEETING_ROOM', 5, 7);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Ostrava Hall 2', 35, 'PRESENTATION_ROOM', 5, 7);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Warsaw Room 1', 50, 'MEETING_ROOM', 6, 12);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Warsaw Room 2', 30, 'PRESENTATION_ROOM', 6, 12);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Krakow Hall A', 45, 'MEETING_ROOM', 7, 12);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Krakow Hall B', 28, 'PRESENTATION_ROOM', 7, 12);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Vienna Room 1', 55, 'MEETING_ROOM', 8, 2);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Vienna Room 2', 32, 'PRESENTATION_ROOM', 8, 2);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Budapest Hall 1', 48, 'MEETING_ROOM', 9, 2);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Budapest Hall 2', 26, 'PRESENTATION_ROOM', 9, 2);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Paris Grand Hall', 80, 'MEETING_ROOM', 10, 1);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Paris Auditorium', 100, 'PRESENTATION_ROOM', 10, 1);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Munich Lab 1', 42, 'MEETING_ROOM', 11, 1);

INSERT INTO rooms ("name", capacity, "type", id_location, id_organizer) 
VALUES ('Munich Lab 2', 38, 'PRESENTATION_ROOM', 11, 1);

COMMIT;

-- Meeting Rooms
INSERT INTO meeting_rooms (id_room, vc_ready) 
SELECT id_room, 'Y' FROM rooms WHERE "name" = 'Room 101';

INSERT INTO meeting_rooms (id_room, vc_ready) 
SELECT id_room, 'Y' FROM rooms WHERE "name" = 'Room 102';

INSERT INTO meeting_rooms (id_room, vc_ready) 
SELECT id_room, 'N' FROM rooms WHERE "name" = 'Room 303';

INSERT INTO meeting_rooms (id_room, vc_ready) 
SELECT id_room, 'Y' FROM rooms WHERE "name" = 'Brno Room A';

INSERT INTO meeting_rooms (id_room, vc_ready) 
SELECT id_room, 'Y' FROM rooms WHERE "name" = 'Ostrava Hall 1';

INSERT INTO meeting_rooms (id_room, vc_ready) 
SELECT id_room, 'Y' FROM rooms WHERE "name" = 'Warsaw Room 1';

INSERT INTO meeting_rooms (id_room, vc_ready) 
SELECT id_room, 'N' FROM rooms WHERE "name" = 'Krakow Hall A';

INSERT INTO meeting_rooms (id_room, vc_ready) 
SELECT id_room, 'Y' FROM rooms WHERE "name" = 'Vienna Room 1';

INSERT INTO meeting_rooms (id_room, vc_ready) 
SELECT id_room, 'Y' FROM rooms WHERE "name" = 'Budapest Hall 1';

INSERT INTO meeting_rooms (id_room, vc_ready) 
SELECT id_room, 'Y' FROM rooms WHERE "name" = 'Paris Grand Hall';

INSERT INTO meeting_rooms (id_room, vc_ready) 
SELECT id_room, 'Y' FROM rooms WHERE "name" = 'Munich Lab 1';

COMMIT;

-- Presentation Rooms
INSERT INTO presentation_rooms (id_room, podium_size) 
SELECT id_room, 13 FROM rooms WHERE "name" = 'Room 103';

INSERT INTO presentation_rooms (id_room, podium_size) 
SELECT id_room, 10 FROM rooms WHERE "name" = 'Room 202';

INSERT INTO presentation_rooms (id_room, podium_size) 
SELECT id_room, 12 FROM rooms WHERE "name" = 'Brno Room B';

INSERT INTO presentation_rooms (id_room, podium_size) 
SELECT id_room, 15 FROM rooms WHERE "name" = 'Ostrava Hall 2';

INSERT INTO presentation_rooms (id_room, podium_size) 
SELECT id_room, 20 FROM rooms WHERE "name" = 'Warsaw Room 2';

INSERT INTO presentation_rooms (id_room, podium_size) 
SELECT id_room, 18 FROM rooms WHERE "name" = 'Krakow Hall B';

INSERT INTO presentation_rooms (id_room, podium_size) 
SELECT id_room, 16 FROM rooms WHERE "name" = 'Vienna Room 2';

INSERT INTO presentation_rooms (id_room, podium_size) 
SELECT id_room, 14 FROM rooms WHERE "name" = 'Budapest Hall 2';

INSERT INTO presentation_rooms (id_room, podium_size) 
SELECT id_room, 30 FROM rooms WHERE "name" = 'Paris Auditorium';

INSERT INTO presentation_rooms (id_room, podium_size) 
SELECT id_room, 22 FROM rooms WHERE "name" = 'Munich Lab 2';

COMMIT;

-- ==============================================
-- ŽÁDOSTI O REZERVACE
-- ==============================================

-- Žádost #1: John Doe - Meeting (úspěch)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 20, 'MEETING_RREQUEST',
        TO_DATE('2024-12-07 09:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-07 11:00', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '2' HOUR, 1, 1);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'Y');

-- Žádost #2: Jane Smith - Presentation (úspěch)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 15, 'PRESENTATION_RREQUEST',
        TO_DATE('2024-12-08 10:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-08 12:00', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '2' HOUR, 2, 2);
INSERT INTO presentation_rrequests (id_room_request, podium_size)
VALUES (room_requests_id_room_request.CURRVAL, 10);

-- Žádost #3: John Doe - Meeting (úspěch)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 25, 'MEETING_RREQUEST',
        TO_DATE('2024-12-09 13:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-09 16:30', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '3:30' HOUR TO MINUTE, 1, 1);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'N');

-- Žádost #4: Jane Smith - Presentation (úspěch)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 20, 'PRESENTATION_RREQUEST',
        TO_DATE('2024-12-10 14:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-10 15:15', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '1:15' HOUR TO MINUTE, 2, 2);
INSERT INTO presentation_rrequests (id_room_request, podium_size)
VALUES (room_requests_id_room_request.CURRVAL, 15);

-- Žádost #5: Peter Parker - Meeting (úspěch)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 15, 'MEETING_RREQUEST',
        TO_DATE('2024-12-11 09:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-11 17:00', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '8' HOUR, 3, 3);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'Y');

-- Žádost #6: Marie Novotná - Meeting (úspěch)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 25, 'MEETING_RREQUEST',
        TO_DATE('2024-12-12 09:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-12 11:00', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '2' HOUR, 1, 5);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'Y');

-- Žádost #7: Marie Novotná - Presentation (úspěch)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 20, 'PRESENTATION_RREQUEST',
        TO_DATE('2024-12-13 14:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-13 16:30', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '2:30' HOUR TO MINUTE, 2, 5);
INSERT INTO presentation_rrequests (id_room_request, podium_size)
VALUES (room_requests_id_room_request.CURRVAL, 10);

-- Žádost #8: Tomáš Svoboda - Meeting (úspěch)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 15, 'MEETING_RREQUEST',
        TO_DATE('2024-12-14 10:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-14 12:00', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '2' HOUR, 3, 6);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'N');

-- Žádost #9: Tomáš Svoboda - Presentation (neúspěch - nedostatek kapacity)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 35, 'PRESENTATION_RREQUEST',
        TO_DATE('2024-12-15 09:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-15 10:30', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '1:30' HOUR TO MINUTE, 2, 6);
INSERT INTO presentation_rrequests (id_room_request, podium_size)
VALUES (room_requests_id_room_request.CURRVAL, 15);

-- Žádost #10: Marie - Meeting (úspěch)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 12, 'MEETING_RREQUEST',
        TO_DATE('2024-12-16 13:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-16 14:30', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '1:30' HOUR TO MINUTE, 3, 5);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'N');

-- Žádost #11: Tomáš - Presentation (úspěch)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 22, 'PRESENTATION_RREQUEST',
        TO_DATE('2024-12-17 11:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-17 13:00', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '2' HOUR, 2, 6);
INSERT INTO presentation_rrequests (id_room_request, podium_size)
VALUES (room_requests_id_room_request.CURRVAL, 10);

-- Žádost #12: Marie - Meeting (úspěch)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 40, 'MEETING_RREQUEST',
        TO_DATE('2024-12-18 09:30', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-18 12:00', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '2:30' HOUR TO MINUTE, 1, 5);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'Y');

-- Žádost #13: Martin Procházka - Meeting (úspěch)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 30, 'MEETING_RREQUEST',
        TO_DATE('2024-12-19 10:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-19 12:00', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '2' HOUR, 4, 8);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'Y');

-- Žádost #14: Lenka Dvořáková - Presentation (úspěch)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 25, 'PRESENTATION_RREQUEST',
        TO_DATE('2024-12-20 14:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-20 16:00', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '2' HOUR, 5, 9);
INSERT INTO presentation_rrequests (id_room_request, podium_size)
VALUES (room_requests_id_room_request.CURRVAL, 15);

-- Žádost #15: Anna Veselá - Meeting (úspěch)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 40, 'MEETING_RREQUEST',
        TO_DATE('2024-12-21 09:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-21 11:30', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '2:30' HOUR TO MINUTE, 6, 11);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'Y');

COMMIT;



-- ==============================================
-- FINÁLNÍ STATISTIKY
-- ==============================================

SELECT 'FINÁLNÍ PŘEHLED DAT' AS INFO FROM DUAL;

SELECT tab_name AS TABULKA, pocet AS POCET FROM (
    SELECT 'COUNTRIES' AS tab_name, COUNT(*) AS pocet FROM countries
    UNION ALL SELECT 'CITIES', COUNT(*) FROM cities
    UNION ALL SELECT 'ORGANIZERS', COUNT(*) FROM organizers
    UNION ALL SELECT 'LOCATIONS', COUNT(*) FROM locations
    UNION ALL SELECT 'ROOMS', COUNT(*) FROM rooms
    UNION ALL SELECT 'MEETING_ROOMS', COUNT(*) FROM meeting_rooms
    UNION ALL SELECT 'PRESENTATION_ROOMS', COUNT(*) FROM presentation_rooms
    UNION ALL SELECT 'ROOM_REQUESTS', COUNT(*) FROM room_requests
    UNION ALL SELECT 'MEETING_RREQUESTS', COUNT(*) FROM meeting_rrequests
    UNION ALL SELECT 'PRESENTATION_RREQUESTS', COUNT(*) FROM presentation_rrequests
    UNION ALL SELECT 'RESERVATIONS', COUNT(*) FROM reservations
    UNION ALL SELECT 'IMAGES', COUNT(*) FROM images
    UNION ALL SELECT 'NOTIFICATIONS', COUNT(*) FROM notifications
) ORDER BY tab_name;

COMMIT;