-- Countries
INSERT INTO countries (id_country, name) VALUES (1, 'Czech Republic');
INSERT INTO countries (id_country, name) VALUES (2, 'Slovakia');
INSERT INTO countries (id_country, name) VALUES (3, 'Germany');

-- Cities
INSERT INTO cities (id_city, name, id_country) VALUES (1, 'Prague', 1);
INSERT INTO cities (id_city, name, id_country) VALUES (2, 'Bratislava', 2);
INSERT INTO cities (id_city, name, id_country) VALUES (3, 'Berlin', 3);

-- Roles
INSERT INTO roles (id_role, name) VALUES (1, 'Administrator');
INSERT INTO roles (id_role, name) VALUES (2, 'User');
INSERT INTO roles (id_role, name) VALUES (3, 'Guest');

-- Organizers
INSERT INTO organizers (id_organizer, name, email, id_role) VALUES (1, 'John Doe', 'john.doe@example.com', 1);
INSERT INTO organizers (id_organizer, name, email, id_role, id_organizer_substitute) VALUES (2, 'Jane Smith', 'jane.smith@example.com', 2, 1);
INSERT INTO organizers (id_organizer, name, email, id_role, id_organizer_substitute) VALUES (3, 'Peter Parker', 'peter.parker@example.com', 3, 1);


-- Locations
INSERT INTO locations (id_location, name, availability_start, availability_end, id_city, id_organizer) 
VALUES (1, 'Main Hall', TO_DATE('2024-12-06 08:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-06 17:00', 'YYYY-MM-DD HH24:MI'), 1, 1);
INSERT INTO locations (id_location, name, availability_start, availability_end, id_city, id_organizer) 
VALUES (2, 'Conference Room A', TO_DATE('2024-12-06 09:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-06 18:00', 'YYYY-MM-DD HH24:MI'), 2, 2);
INSERT INTO locations (id_location, name, availability_start, availability_end, id_city, id_organizer) 
VALUES (3, 'Berlin Office', TO_DATE('2024-12-06 10:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-06 19:00', 'YYYY-MM-DD HH24:MI'), 3, 3);
-- Rooms
INSERT INTO rooms (id_room, name, capacity, type, id_location, id_organizer) 
VALUES (1, 'Room 101', 50, 'MEETING_ROOM', 1, 1);
INSERT INTO rooms (id_room, name, capacity, type, id_location, id_organizer) 
VALUES (2, 'Room 202', 30, 'PRESENTATION_ROOM', 2, 2);
INSERT INTO rooms (id_room, name, capacity, type, id_location, id_organizer) 
VALUES (3, 'Room 303', 20, 'MEETING_ROOM', 3, 3);

-- Meeting Rooms
INSERT INTO meeting_rooms (id_room, vc_ready) VALUES (1, 'Y');
INSERT INTO meeting_rooms (id_room, vc_ready) VALUES (3, 'N');

-- Presentation Rooms
INSERT INTO presentation_rooms (id_room, podium_size) VALUES (2, 10);

-- Room Requests
INSERT INTO room_requests (id_room_request, min_capacity, type, reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (1, 20, 'MEETING_RREQUEST', TO_DATE('2024-12-07 09:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-07 11:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('02:00', 'HH24:MI'), 1, 1);
INSERT INTO room_requests (id_room_request, min_capacity, type, reservation_start, reservation_end, reservation_length, id_location, id_organizer) 
VALUES (2, 15, 'PRESENTATION_RREQUEST', TO_DATE('2024-12-08 10:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-08 12:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('02:00', 'HH24:MI'), 2, 2);

-- Meeting Room Requests
INSERT INTO meeting_rrequests (id_room_request, vc_ready) VALUES (1, 'Y');

-- Presentation Room Requests
INSERT INTO presentation_rrequests (id_room_request, podium_size) VALUES (2, 10);


-- Reservations
INSERT INTO reservations (id_reservation, "start", end, id_room, id_room_request, id_organizer) 
VALUES (1, TO_DATE('2024-12-09 10:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-09 12:00', 'YYYY-MM-DD HH24:MI'), 1, 1, 1);
INSERT INTO reservations (id_reservation, "start", end, id_room, id_room_request, id_organizer) 
VALUES (2, TO_DATE('2024-12-10 13:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-12-10 15:00', 'YYYY-MM-DD HH24:MI'), 2, 2, 2);

-- Notifications
INSERT INTO notifications (id_notification, notification_type, delivered, id_organizer, id_room_request) 
VALUES (1, 'Email', 'Y', 1, 1);
INSERT INTO notifications (id_notification, notification_type, delivered, id_organizer, id_room_request) 
VALUES (2, 'SMS', 'N', 2, 2);

-- Credentials
INSERT INTO credentials (id_credential, credential_type, data, created_at, id_organizer) 
VALUES (1, 'PASSWORD', 'hashed_password_123', SYSTIMESTAMP, 1);
INSERT INTO credentials (id_credential, credential_type, data, created_at, id_organizer) 
VALUES (2, 'PASSWORD', 'hashed_password_456', SYSTIMESTAMP, 2);
INSERT INTO credentials (id_credential, credential_type, data, created_at, id_organizer) 
VALUES (3, 'PASSWORD', 'hashed_password_789', SYSTIMESTAMP, 3);

-- Images
INSERT INTO images (id_image, data, id_organizer, id_location, id_room) 
VALUES (1, EMPTY_BLOB(), 1, 1, 1);
INSERT INTO images (id_image, data, id_organizer, id_location, id_room) 
VALUES (2, EMPTY_BLOB(), 2, 2, 2);
INSERT INTO images (id_image, data, id_organizer, id_location, id_room) 
VALUES (3, EMPTY_BLOB(), 3, 3, 3);

COMMIT;