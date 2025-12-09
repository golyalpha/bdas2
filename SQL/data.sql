-- Countries
INSERT INTO countries ("name") VALUES ('Czech Republic');
INSERT INTO countries ("name") VALUES ('Slovakia');
INSERT INTO countries ("name") VALUES ('Germany');

-- Cities
INSERT INTO cities ("name", id_country) VALUES ('Prague', 1);
INSERT INTO cities ("name", id_country) VALUES ('Bratislava', 2);
INSERT INTO cities ("name", id_country) VALUES ('Berlin', 3);


INSERT INTO notification_types ("code", "name", "description" ) VALUES ('ALLOC_SUCCESS', 'Alokace úspěšná', 'Vaše žádost o rezervaci místnosti byla úspěšně alokována.');

INSERT INTO notification_types ("code", "name", "description" ) VALUES ('ALLOC_FAIL', 'Alokace neúspěšná', 'Vaše žádost o rezervaci místnosti nebyla alokována.');

-- Roles
INSERT INTO roles ("name") VALUES ('Administrator');
INSERT INTO roles ("name") VALUES ('User');
INSERT INTO roles ("name") VALUES ('Guest');

-- Organizers
INSERT INTO organizers ("name", email, id_role) VALUES ('John Doe', 'john.doe@example.com', 1);
INSERT INTO organizers ("name", email, id_role, id_organizer_substitute) VALUES ('Jane Smith', 'jane.smith@example.com', 2, 1);
INSERT INTO organizers ("name", email, id_role, id_organizer_substitute) VALUES ('Peter Parker', 'peter.parker@.com', 3, 1);

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

-- Meeting Rooms (navázání přes poddotaz na ID místnosti)
INSERT INTO meeting_rooms (id_room, vc_ready) 
SELECT id_room, 'Y' FROM rooms WHERE "name" = 'Room 101';
INSERT INTO meeting_rooms (id_room, vc_ready) 
SELECT id_room, 'N' FROM rooms WHERE "name" = 'Room 303';

-- Presentation Rooms
INSERT INTO presentation_rooms (id_room, podium_size) 
SELECT id_room, 10 FROM rooms WHERE "name" = 'Room 202';

-- MEETING_RREQUEST #1: 2 hodiny (09:00 - 11:00)
-- Požadavek na VC = Y -> vybere se Room 101 (mr.vc_ready = 'Y', kapacita 50)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 20, 'MEETING_RREQUEST',
        TO_DATE('2024-12-07 09:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-07 11:00', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '2' HOUR,  -- ZMĚNA: INTERVAL místo TO_DATE
        1, 1);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'Y');

-- PRESENTATION_RREQUEST #1: 2 hodiny (10:00 - 12:00)
-- Pódium 10 -> vybere se Room 202 (podium_size = 10, kapacita 30)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 15, 'PRESENTATION_RREQUEST',
        TO_DATE('2024-12-08 10:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-08 12:00', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '2' HOUR,  -- ZMĚNA: INTERVAL místo TO_DATE
        2, 2);
INSERT INTO presentation_rrequests (id_room_request, podium_size)
VALUES (room_requests_id_room_request.CURRVAL, 10);

-- MEETING_RREQUEST #2: 3 hodiny 30 minut (13:00 - 16:30) - test různých délek
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 25, 'MEETING_RREQUEST',
        TO_DATE('2024-12-09 13:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-09 16:30', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '3:30' HOUR TO MINUTE,  -- 3 hodiny 30 minut
        1, 1);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'N');

-- PRESENTATION_RREQUEST #2: 1 hodina 15 minut (14:00 - 15:15)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 20, 'PRESENTATION_RREQUEST',
        TO_DATE('2024-12-10 14:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-10 15:15', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '1:15' HOUR TO MINUTE,  -- 1 hodina 15 minut
        2, 2);
INSERT INTO presentation_rrequests (id_room_request, podium_size)
VALUES (room_requests_id_room_request.CURRVAL, 15);

-- MEETING_RREQUEST #3: Celý den (8 hodin)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 30, 'MEETING_RREQUEST',
        TO_DATE('2024-12-11 09:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-11 17:00', 'YYYY-MM-DD HH24:MI'),
        INTERVAL '8' HOUR,  -- Celý pracovní den
        3, 3);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'Y');

-- Credentials
INSERT INTO credentials (credential_type, "data", created_at, id_organizer) 
VALUES ('PASSWORD', 'hashed_password_123', SYSTIMESTAMP, 1);
INSERT INTO credentials (credential_type, "data", created_at, id_organizer) 
VALUES ('PASSWORD', 'hashed_password_456', SYSTIMESTAMP, 2);
INSERT INTO credentials (credential_type, "data", created_at, id_organizer) 
VALUES ('PASSWORD', 'hashed_password_789', SYSTIMESTAMP, 3);

-- Images s novými poli: file_name, file_suffix, created_at
INSERT INTO images ("data", file_name, file_suffix, created_at, id_organizer, id_location, id_room) 
VALUES (EMPTY_BLOB(), 'room_101_default', 'jpg', SYSDATE, 1, 1, 1);

INSERT INTO images ("data", file_name, file_suffix, created_at, id_organizer, id_location, id_room) 
VALUES (EMPTY_BLOB(), 'room_202_default', 'png', SYSDATE, 2, 2, 2);

INSERT INTO images ("data", file_name, file_suffix, created_at, id_organizer, id_location, id_room) 
VALUES (EMPTY_BLOB(), 'room_303_default', 'jpg', SYSDATE, 3, 3, 3);

COMMIT;