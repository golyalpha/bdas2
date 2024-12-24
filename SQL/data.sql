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

-- Meeting Rooms
INSERT INTO meeting_rooms (id_room, vc_ready) VALUES (1, 'Y');
INSERT INTO meeting_rooms (id_room, vc_ready) VALUES (3, 'N');

-- Presentation Rooms
INSERT INTO presentation_rooms (id_room, podium_size) VALUES (2,10);

-- Room Requests
INSERT INTO room_requests (min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (20, 'MEETING_RREQUEST', TO_DATE('2024-12-07 09:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-07 11:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('02:00', 'HH24:MI'), 1, 1);
INSERT INTO room_requests (min_capacity, "type", reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (15, 'PRESENTATION_RREQUEST', TO_DATE('2024-12-08 10:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-08 12:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('02:00', 'HH24:MI'), 2, 2);

-- Meeting Room Requestsexample
INSERT INTO meeting_rrequests (id_room_request, vc_ready) VALUES (1,'Y');

-- Presentation Room Requests
INSERT INTO presentation_rrequests (id_room_request, podium_size) VALUES (2, 10);

-- Reservations
INSERT INTO reservations ("start", "end", id_room, id_room_request, id_organizer) 
VALUES (TO_DATE('2024-12-09 10:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-09 12:00', 'YYYY-MM-DD HH24:MI'), 1, 1, 1);
INSERT INTO reservations ("start", "end", id_room, id_room_request, id_organizer) 
VALUES (TO_DATE('2024-12-10 13:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-10 15:00', 'YYYY-MM-DD HH24:MI'), 2, 2, 2);

-- Notifications
INSERT INTO notifications (notification_type, delivered, id_organizer, id_room_request) 
VALUES ('Email', 'Y', 1, 1);
INSERT INTO notifications (notification_type, delivered, id_organizer, id_room_request) 
VALUES ('SMS', 'N', 2, 2);

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