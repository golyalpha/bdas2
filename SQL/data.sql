-- Countries
INSERT INTO countries ("name") VALUES ('Czech Republic');
INSERT INTO countries ("name") VALUES ('Slovakia');
INSERT INTO countries ("name") VALUES ('Germany');

-- Cities
INSERT INTO cities ("name", id_country) VALUES ('Prague', 1);
INSERT INTO cities ("name", id_country) VALUES ('Bratislava', 2);
INSERT INTO cities ("name", id_country) VALUES ('Berlin', 3);

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

-- Meeting Rooms (navázání pøes poddotaz na ID místnosti)
INSERT INTO meeting_rooms (id_room, vc_ready) 
SELECT id_room, 'Y' FROM rooms WHERE "name" = 'Room 101';
INSERT INTO meeting_rooms (id_room, vc_ready) 
SELECT id_room, 'N' FROM rooms WHERE "name" = 'Room 303';

-- Presentation Rooms
INSERT INTO presentation_rooms (id_room, podium_size) 
SELECT id_room, 10 FROM rooms WHERE "name" = 'Room 202';

-- Room Requests
-- Room Requests
-- MEETING_RREQUEST, požadavek na VC = Y -> vybere se Room 101 (mr.vc_ready = 'Y', kapacita 50)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 20, 'MEETING_RREQUEST',
        TO_DATE('2024-12-07 09:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-07 11:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('02:00', 'HH24:MI'), 1, 1);
INSERT INTO meeting_rrequests (id_room_request, vc_ready)
VALUES (room_requests_id_room_request.CURRVAL, 'Y');

-- PRESENTATION_RREQUEST, pódium 10 -> vybere se Room 202 (podium_size = 10, kapacita 30)
INSERT INTO room_requests (id_room_request, min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (room_requests_id_room_request.NEXTVAL, 15, 'PRESENTATION_RREQUEST',
        TO_DATE('2024-12-08 10:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('2024-12-08 12:00', 'YYYY-MM-DD HH24:MI'),
        TO_DATE('02:00', 'HH24:MI'), 2, 2);
INSERT INTO presentation_rrequests (id_room_request, podium_size)
VALUES (room_requests_id_room_request.CURRVAL, 10);


-- Credentials
INSERT INTO credentials (credential_type, "data", created_at, id_organizer) 
VALUES ('PASSWORD', 'hashed_password_123', SYSTIMESTAMP, 1);
INSERT INTO credentials (credential_type, "data", created_at, id_organizer) 
VALUES ('PASSWORD', 'hashed_password_456', SYSTIMESTAMP, 2);
INSERT INTO credentials (credential_type, "data", created_at, id_organizer) 
VALUES ('PASSWORD', 'hashed_password_789', SYSTIMESTAMP, 3);

-- Images
INSERT INTO images ("data", id_organizer, id_location, id_room) 
VALUES (EMPTY_BLOB(), 1, 1, 1);
INSERT INTO images ("data", id_organizer, id_location, id_room) 
VALUES (EMPTY_BLOB(), 2, 2, 2);
INSERT INTO images ("data", id_organizer, id_location, id_room) 
VALUES (EMPTY_BLOB(), 3, 3, 3);
COMMIT;