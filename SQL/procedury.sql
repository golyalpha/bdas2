CREATE OR REPLACE PACKAGE reservations_pkg AS
    PROCEDURE edit_room (
        p_id_room IN NUMBER DEFAULT NULL,
        p_name IN VARCHAR2,
        p_capacity IN NUMBER,
        p_type IN VARCHAR2,
        p_id_location IN NUMBER,
        p_id_organizer IN NUMBER,
        p_vc_ready IN CHAR DEFAULT NULL, -- pro MEETING_ROOM
        p_podium_size IN NUMBER DEFAULT NULL -- pro PRESENTATION_ROOM
    );

    PROCEDURE edit_organizer(
        p_id_organizer IN NUMBER DEFAULT NULL,
        p_name IN VARCHAR2,
        p_email IN VARCHAR2,
        p_id_role IN NUMBER DEFAULT NULL,
        p_id_organizer_substitute IN NUMBER DEFAULT NULL
    );

    PROCEDURE edit_credential(
        p_id_credential IN NUMBER DEFAULT NULL,
        p_credential_type IN CHAR,
        p_data IN VARCHAR2,
        p_id_organizer IN NUMBER
    );

    PROCEDURE edit_reservation(
        p_id_reservation IN NUMBER DEFAULT NULL,
        p_start IN DATE,
        p_end IN DATE,
        p_id_room IN NUMBER,
        p_id_room_request IN NUMBER,
        p_id_organizer IN NUMBER
    );

    PROCEDURE edit_location(
        p_id_location IN NUMBER DEFAULT NULL,
        p_name IN VARCHAR2,
        p_start IN DATE,
        p_end IN DATE,
        p_id_city IN NUMBER,
        p_id_organizer IN NUMBER
    );

    -- DELETE procedures
    PROCEDURE delete_room (
        p_id_room IN NUMBER
    );

    PROCEDURE delete_location (
        p_id_location IN NUMBER
    );

    PROCEDURE delete_reservation (
        p_id_reservation IN NUMBER
    );

    PROCEDURE delete_room_request (
        p_id_room_request IN NUMBER
    );

END reservations_pkg;
/

CREATE OR REPLACE PACKAGE BODY reservations_pkg AS
    -- Edit or Create Location
    PROCEDURE edit_location(
        p_id_location IN NUMBER DEFAULT NULL,
        p_name IN VARCHAR2,
        p_start IN DATE,
        p_end IN DATE,
        p_id_city IN NUMBER,
        p_id_organizer IN NUMBER
    ) IS
        v_new_id NUMBER; -- For new location ID
    BEGIN
        IF p_id_location IS NULL THEN
            -- Generate new ID using sequence
            SELECT locations_id_location_seq.NEXTVAL INTO v_new_id FROM dual;

            -- Insert new location
            INSERT INTO locations (
                id_location, name, availability_start, availability_end, id_city, id_organizer
            ) VALUES (
                v_new_id, p_name, p_start, p_end, p_id_city, p_id_organizer
            );

        ELSE
            -- Update existing location
            UPDATE locations
            SET 
                name = p_name,
                availability_start = p_start,
                availability_end = p_end,
                id_city = p_id_city,
                id_organizer = p_id_organizer
            WHERE id_location = p_id_location;

            -- Ensure the update affected rows
            IF SQL%ROWCOUNT = 0 THEN
                RAISE_APPLICATION_ERROR(-20001, 'No location found with the given ID.');
            END IF;
        END IF;

        -- Commit the transaction
        COMMIT;
    END edit_location;

    -- EDIT ROOM
    PROCEDURE edit_room (
        p_id_room IN NUMBER DEFAULT NULL,
        p_name IN VARCHAR2,
        p_capacity IN NUMBER,
        p_type IN VARCHAR2,
        p_id_location IN NUMBER,
        p_id_organizer IN NUMBER,
        p_vc_ready IN CHAR DEFAULT NULL,
        p_podium_size IN NUMBER DEFAULT NULL
    ) IS
        v_new_id NUMBER;
    BEGIN
        -- Kontrola typu místnosti
        IF p_type NOT IN ('MEETING_ROOM', 'PRESENTATION_ROOM') THEN
            RAISE_APPLICATION_ERROR(-20003, 'Invalid room type.');
        END IF;

        -- Pro nový záznam
        IF p_id_room IS NULL THEN
            SELECT rooms_id_room_seq.NEXTVAL INTO v_new_id FROM dual;

            INSERT INTO rooms (id_room, name, capacity, type, id_location, id_organizer)
            VALUES (v_new_id, p_name, p_capacity, p_type, p_id_location, p_id_organizer);

            IF p_type = 'MEETING_ROOM' THEN
                INSERT INTO meeting_rooms (id_room, vc_ready)
                VALUES (v_new_id, p_vc_ready);
            ELSIF p_type = 'PRESENTATION_ROOM' THEN
                INSERT INTO presentation_rooms (id_room, podium_size)
                VALUES (v_new_id, p_podium_size);
            END IF;

        -- Pro aktualizaci
        ELSE
            UPDATE rooms
            SET name = p_name,
                capacity = p_capacity,
                type = p_type,
                id_location = p_id_location,
                id_organizer = p_id_organizer
            WHERE id_room = p_id_room;

            IF p_type = 'MEETING_ROOM' THEN
                UPDATE meeting_rooms
                SET vc_ready = p_vc_ready
                WHERE id_room = p_id_room;
            ELSIF p_type = 'PRESENTATION_ROOM' THEN
                UPDATE presentation_rooms
                SET podium_size = p_podium_size
                WHERE id_room = p_id_room;
            END IF;
        END IF;

        COMMIT;
    END edit_room;

    -- EDIT RESERVATION
PROCEDURE edit_reservation(
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
END edit_reservation;

PROCEDURE edit_organizer (
    p_id_organizer IN NUMBER DEFAULT NULL,
    p_name IN VARCHAR2,
    p_email IN VARCHAR2,
    p_id_role IN NUMBER DEFAULT NULL,
    p_id_organizer_substitute IN NUMBER DEFAULT NULL    
    ) IS
        v_new_id NUMBER;
    BEGIN
        IF p_id_organizer IS NULL THEN
            SELECT organizers_id_organizer_seq.NEXTVAL INTO v_new_id FROM dual;

            INSERT INTO organizers (id_organizer, name, email, id_role, id_organizer_substitute)
            VALUES (v_new_id, p_name, p_email, p_id_role, p_id_organizer_substitute);
        ELSE
            UPDATE organizers
            SET name = p_name,
                email = p_email,
                id_role = p_id_role,
                id_organizer_substitute = p_id_organizer_substitute
            WHERE id_organizer = p_id_organizer;

            IF SQL%ROWCOUNT = 0 THEN
                RAISE_APPLICATION_ERROR(-20001, 'No organizer found with the given ID.');
            END IF;
        END IF;

        COMMIT;
    END edit_organizer;

    -- DELETE ROOM
    PROCEDURE delete_room (
        p_id_room IN NUMBER
    ) IS
    BEGIN
        DELETE FROM meeting_rooms WHERE id_room = p_id_room;
        DELETE FROM presentation_rooms WHERE id_room = p_id_room;

        DELETE FROM rooms WHERE id_room = p_id_room;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20002, 'Room with ID ' || p_id_room || ' does not exist.');
        END IF;

        DBMS_OUTPUT.PUT_LINE('Room deleted successfully.');
        COMMIT;
    END delete_room;

    -- DELETE LOCATION
    PROCEDURE delete_location (
        p_id_location IN NUMBER
    ) IS
    BEGIN
        
        DELETE FROM locations WHERE id_location = p_id_location;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20002, 'Location with ID ' || p_id_location || ' does not exist.');
        END IF;

        DBMS_OUTPUT.PUT_LINE('Location deleted successfully.');
        COMMIT;
    END delete_location;

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


    -- DELETE ROOM REQUEST
    PROCEDURE delete_room_request (
        p_id_room_request IN NUMBER
    ) IS
    BEGIN
        DELETE FROM PRESENTATION_RREQUESTS WHERE id_room_request = p_id_room_request;
        DELETE FROM MEETING_RREQUESTS WHERE id_room_request = p_id_room_request;

        DELETE FROM room_requests WHERE id_room_request = p_id_room_request;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20002, 'Room request with ID ' || p_id_room_request || ' does not exist.');
        END IF;

        DBMS_OUTPUT.PUT_LINE('Room request deleted successfully.');
        COMMIT;
    END delete_room_request;

    PROCEDURE edit_credential(
        p_id_credential IN NUMBER DEFAULT NULL,
        p_credential_type IN CHAR,
        p_data IN VARCHAR2,
        p_id_organizer IN NUMBER
    ) IS
        v_new_id NUMBER;
    BEGIN
        IF p_id_credential IS NULL THEN
            SELECT credentials_id_credential_seq.NEXTVAL INTO v_new_id FROM dual;

            INSERT INTO credentials (id_credential, credential_type, data, created_at, id_organizer)
            VALUES (v_new_id, p_credential_type, p_data, CURRENT_TIMESTAMP, p_id_organizer);
        ELSE
            RAISE_APPLICATION_ERROR(-20001, 'Credentials are not mutable.');
        END IF;

        COMMIT;
    END edit_credential;

END reservations_pkg;
/

BEGIN
    reservations_pkg.edit_location(
        p_id_location => NULL,
        p_name => 'aaaadddddddddddaaaal',
        p_start => TO_DATE('2024-12-06 10:00', 'YYYY-MM-DD HH24:MI'),
        p_end => TO_DATE('2024-12-06 19:00', 'YYYY-MM-DD HH24:MI'),
        p_id_city => 3,
        p_id_organizer => 3
    );
END;
/


BEGIN
    RESERVATIONS_PKG.EDIT_ROOM(
        P_ID_ROOM => NULL,
        P_NAME => 'Room 10DAHFKJAFHDK1',
        P_CAPACITY => 50,
        P_TYPE => 'MEETING_ROOM',
        P_ID_LOCATION => 1,
        P_ID_ORGANIZER => 1,
        P_VC_READY => 'Y'
    );
END;
/




BEGIN
    reservations_pkg.edit_reservation(
        p_id_reservation => NULL,
        p_start => TO_DATE('2024-01-10 14:00:00' , 'YYYY-MM-DD HH24:MI:SS'),
        p_end => TO_DATE('2024-01-10 16:00:00', 'YYYY-MM-DD HH24:MI:SS'),
        p_id_room => 1,
        p_id_room_request => 1,
        p_id_organizer => 1
    );
END;
/





BEGIN
    reservations_pkg.edit_reservation(
        p_id_reservation => NULL,   -- Pass NULL if inserting a new reservation
        p_start => SYSDATE,        -- Replace with a valid DATE value
        p_end => SYSDATE + 2,      -- Replace with a valid DATE value for end
        p_id_room => 1,            -- Replace with a valid Room ID
        p_id_room_request => 1,    -- Replace with a valid Room Request ID
        p_id_organizer => 1        -- Replace with a valid Organizer ID
    );
END;
/


/*nejaky problemy s Non Transferable FK constraintama
BEGIN
    reservations_pkg.edit_reservation(
        p_id_reservation => 2,   -- Pass NULL if inserting a new reservation
        p_start => SYSDATE,        -- Replace with a valid DATE value
        p_end => SYSDATE + 2,      -- Replace with a valid DATE value for end
        p_id_room => 1,            -- Replace with a valid Room ID
        p_id_room_request => 1,    -- Replace with a valid Room Request ID
        p_id_organizer => 1        -- Replace with a valid Organizer ID
    );
END;
/*




BEGIN 
    reservations_pkg.DELETE_LOCATION(1);
END;


BEGIN 
    reservations_pkg.DELETE_LOCATION(1);
END;
/

BEGIN 
    reservations_pkg.DELETE_ROOM(2);
END;
/

BEGIN 
    reservations_pkg.DELETE_ROOM_REQUEST(1);
END;
/

BEGIN 
    reservations_pkg.DELETE_RESERVATION(2);
END;
/

BEGIN
    reservations_pkg.edit_room(
        p_name => 'Meeting Room 1000000000ídá0000',
        p_capacity => 50,
        p_type => 'MEETING_ROOM',
        p_id_location => 2,
        p_id_organizer => 2,
        p_vc_ready => 'Y' -- CHAR(1): 'Y' nebo 'N'
    );
END;
/

BEGIN
    reservations_pkg.edit_room(
        p_name => 'Presentation Room 2089892',
        p_capacity => 100,
        p_type => 'PRESENTATION_ROOM',
        p_id_location => 2,
        p_id_organizer => 3,
        p_podium_size => 25 -- Velikost podia
    );
END;
/

BEGIN
    reservations_pkg.edit_room(
        p_id_room => 2,
        p_name => 'Meeting R',
        p_capacity => 50,
        p_type => 'PRESENTATION_ROOM',
        p_id_location => 1,
        p_id_organizer => 2,
        p_podium_size => 25 -- Velikost podia
    );
END;






BEGIN
    reservations_pkg.edit_room(
        p_id_room => NULL,
        p_name => 'Test Presentation Room',
        p_capacity => 50,
        p_type => 'PRESENTATION_ROOM',
        p_id_location => 1,
        p_id_organizer => 2,
        p_podium_size => 100
    );
END;
/
-- Generated by Oracle SQL Developer Data Modeler 23.1.0.087.0806
--   at:        2024-12-06 11:49:33 SE�
--   site:      Oracle Database 11g
--   type:      Oracle Database 11g

DROP SEQUENCE cities_id_city_seq;

DROP SEQUENCE countries_id_country_seq;

DROP SEQUENCE credentials_id_credential_seq;

DROP SEQUENCE images_id_image_seq;

DROP SEQUENCE locations_id_location_seq;

DROP SEQUENCE notifications_id_notification;

DROP SEQUENCE organizers_id_organizer_seq;

DROP SEQUENCE reservations_id_reservation;

DROP SEQUENCE roles_id_role_seq;

DROP SEQUENCE room_requests_id_room_request;

DROP SEQUENCE rooms_id_room_seq;

DROP VIEW CITIES_V CASCADE CONSTRAINTS 
;

DROP VIEW COUNTRIES_V CASCADE CONSTRAINTS 
;

DROP VIEW CREDENTIALS_V CASCADE CONSTRAINTS 
;

DROP VIEW IMAGES_V CASCADE CONSTRAINTS 
;

DROP VIEW LOCATIONS_V CASCADE CONSTRAINTS 
;

DROP VIEW NOTIFICATIONS_V CASCADE CONSTRAINTS 
;

DROP VIEW ORGANIZERS_V CASCADE CONSTRAINTS 
;

DROP VIEW RESERVATIONS_V CASCADE CONSTRAINTS 
;

DROP VIEW ROLES_V CASCADE CONSTRAINTS 
;

DROP VIEW ROOM_REQUESTS_V CASCADE CONSTRAINTS 
;

DROP VIEW ROOMS_V CASCADE CONSTRAINTS 
;

DROP VIEW USERS_V CASCADE CONSTRAINTS 
;

DROP TABLE cities CASCADE CONSTRAINTS;

DROP TABLE countries CASCADE CONSTRAINTS;

DROP TABLE credentials CASCADE CONSTRAINTS;

DROP TABLE images CASCADE CONSTRAINTS;

DROP TABLE locations CASCADE CONSTRAINTS;

DROP TABLE meeting_rooms CASCADE CONSTRAINTS;

DROP TABLE meeting_rrequests CASCADE CONSTRAINTS;

DROP TABLE notifications CASCADE CONSTRAINTS;

DROP TABLE organizers CASCADE CONSTRAINTS;

DROP TABLE presentation_rooms CASCADE CONSTRAINTS;

DROP TABLE presentation_rrequests CASCADE CONSTRAINTS;

DROP TABLE reservations CASCADE CONSTRAINTS;

DROP TABLE roles CASCADE CONSTRAINTS;

DROP TABLE room_requests CASCADE CONSTRAINTS;

DROP TABLE rooms CASCADE CONSTRAINTS;

-- predefined type, no DDL - MDSYS.SDO_GEOMETRY

-- predefined type, no DDL - XMLTYPE

CREATE TABLE cities (
    id_city    NUMBER NOT NULL,
    name       VARCHAR2(64) NOT NULL,
    id_country NUMBER NOT NULL
);

ALTER TABLE cities ADD CONSTRAINT cities_pk PRIMARY KEY ( id_city );

ALTER TABLE cities ADD CONSTRAINT city_name_id_country_un UNIQUE ( name,
                                                                   id_country );

CREATE TABLE countries (
    id_country NUMBER NOT NULL,
    name       VARCHAR2(32) NOT NULL
);

ALTER TABLE countries ADD CONSTRAINT countries_pk PRIMARY KEY ( id_country );

ALTER TABLE countries ADD CONSTRAINT countries_name_un UNIQUE ( name );

CREATE TABLE credentials (
    id_credential   INTEGER NOT NULL,
    credential_type CHAR(8) NOT NULL,
    data            VARCHAR2(256) NOT NULL,
    created_at      TIMESTAMP WITH LOCAL TIME ZONE NOT NULL,
    id_organizer    NUMBER NOT NULL
);

ALTER TABLE credentials ADD CONSTRAINT credentials_pk PRIMARY KEY ( id_credential );

CREATE TABLE images (
    id_image     NUMBER NOT NULL,
    data         BLOB NOT NULL,
    id_organizer NUMBER NOT NULL,
    id_location  NUMBER NOT NULL,
    id_room      NUMBER NOT NULL
);

CREATE UNIQUE INDEX image__idx ON
    images (
        id_location
    ASC );

CREATE UNIQUE INDEX image__idxv1 ON
    images (
        id_organizer
    ASC );

CREATE UNIQUE INDEX image__idxv2 ON
    images (
        id_room
    ASC );

ALTER TABLE images ADD CONSTRAINT images_pk PRIMARY KEY ( id_image );

CREATE TABLE locations (
    id_location        NUMBER NOT NULL,
    name               VARCHAR2(32) NOT NULL,
    availability_start DATE NOT NULL,
    availability_end   DATE NOT NULL,
    id_city            NUMBER NOT NULL,
    id_organizer       NUMBER NOT NULL
);

ALTER TABLE locations ADD CONSTRAINT locations_pk PRIMARY KEY ( id_location );

ALTER TABLE locations ADD CONSTRAINT location_name_id_city_un UNIQUE ( name,
                                                                       id_city );

CREATE TABLE meeting_rooms (
    id_room  NUMBER NOT NULL,
    vc_ready CHAR(1) NOT NULL
);

ALTER TABLE meeting_rooms ADD CONSTRAINT meeting_room_pk PRIMARY KEY ( id_room );

CREATE TABLE meeting_rrequests (
    id_room_request NUMBER NOT NULL,
    vc_ready        CHAR(1)
);

ALTER TABLE meeting_rrequests ADD CONSTRAINT meeting_rrequest_pk PRIMARY KEY ( id_room_request );

CREATE TABLE notifications (
    id_notification   INTEGER NOT NULL,
    notification_type VARCHAR2(16) NOT NULL,
    delivered         CHAR(1) NOT NULL,
    id_organizer      NUMBER NOT NULL,
    id_room_request   NUMBER NOT NULL
);

CREATE UNIQUE INDEX notification__idx ON
    notifications (
        id_room_request
    ASC );

ALTER TABLE notifications ADD CONSTRAINT notifications_pk PRIMARY KEY ( id_notification );

CREATE TABLE organizers (
    id_organizer            NUMBER NOT NULL,
    name                    VARCHAR2(64) NOT NULL,
    email                   VARCHAR2(320) NOT NULL,
    id_role                 INTEGER,
    id_organizer_substitute NUMBER
);


ALTER TABLE organizers
    ADD CONSTRAINT organizers_organizers_fk FOREIGN KEY ( id_organizer )
        REFERENCES organizers ( id_organizer );

ALTER TABLE organizers ADD CONSTRAINT organizers_pk PRIMARY KEY ( id_organizer );

CREATE TABLE presentation_rooms (
    id_room     NUMBER NOT NULL,
    podium_size NUMBER NOT NULL
);

ALTER TABLE presentation_rooms ADD CONSTRAINT presentation_room_pk PRIMARY KEY ( id_room );

CREATE TABLE presentation_rrequests (
    id_room_request NUMBER NOT NULL,
    podium_size     NUMBER
);

ALTER TABLE presentation_rrequests ADD CONSTRAINT presentation_rrequest_pk PRIMARY KEY ( id_room_request );

CREATE TABLE reservations (
    id_reservation  NUMBER NOT NULL,
    "start"         DATE NOT NULL,
    "end"             DATE NOT NULL,
    id_room         NUMBER NOT NULL,
    id_room_request NUMBER NOT NULL,
    id_organizer    NUMBER NOT NULL
);

CREATE UNIQUE INDEX reservation__idx ON
    reservations (
        id_room_request
    ASC );

ALTER TABLE reservations ADD CONSTRAINT reservations_pk PRIMARY KEY ( id_reservation );

CREATE TABLE roles (
    id_role INTEGER NOT NULL,
    name    VARCHAR2(32) NOT NULL
);

ALTER TABLE roles ADD CONSTRAINT roles_pk PRIMARY KEY ( id_role );

CREATE TABLE room_requests (
    id_room_request    NUMBER NOT NULL,
    min_capacity       NUMBER NOT NULL,
    type               VARCHAR2(21) NOT NULL,
    reservation_start  DATE NOT NULL,
    reservation_end    DATE,
    reservation_length DATE,
    id_location        NUMBER,
    id_organizer       NUMBER NOT NULL
);

ALTER TABLE room_requests
    ADD CONSTRAINT ch_inh_room_request CHECK ( type IN ( 'MEETING_RREQUEST', 'PRESENTATION_RREQUEST', 'ROOM_REQUEST' ) );

ALTER TABLE room_requests ADD CONSTRAINT room_request_pk PRIMARY KEY ( id_room_request );

CREATE TABLE rooms (
    id_room      NUMBER NOT NULL,
    name         VARCHAR2(32) NOT NULL,
    capacity     NUMBER NOT NULL,
    type         VARCHAR2(17) NOT NULL,
    id_location  NUMBER NOT NULL,
    id_organizer NUMBER NOT NULL
);

ALTER TABLE rooms
    ADD CONSTRAINT ch_inh_room CHECK ( type IN ( 'MEETING_ROOM', 'PRESENTATION_ROOM', 'ROOM' ) );

ALTER TABLE rooms ADD CONSTRAINT room_pk PRIMARY KEY ( id_room ) ;

ALTER TABLE rooms ADD CONSTRAINT rooms_name_un UNIQUE ( name );

ALTER TABLE cities
    ADD CONSTRAINT city_country_fk FOREIGN KEY ( id_country )
        REFERENCES countries ( id_country ) ON DELETE CASCADE;

ALTER TABLE credentials
    ADD CONSTRAINT credential_organizers_fk FOREIGN KEY ( id_organizer )
        REFERENCES organizers ( id_organizer ) ON DELETE CASCADE;

ALTER TABLE images
    ADD CONSTRAINT images_location_fk FOREIGN KEY ( id_location )
        REFERENCES locations ( id_location ) ON DELETE CASCADE;

ALTER TABLE images
    ADD CONSTRAINT images_organizers_fk FOREIGN KEY ( id_organizer )
        REFERENCES organizers ( id_organizer ) ON DELETE CASCADE;

ALTER TABLE images
    ADD CONSTRAINT images_room_fk FOREIGN KEY ( id_room )
        REFERENCES rooms ( id_room ) ON DELETE CASCADE;

ALTER TABLE locations
    ADD CONSTRAINT location_city_fk FOREIGN KEY ( id_city )
        REFERENCES cities ( id_city ) ON DELETE CASCADE;

ALTER TABLE locations
    ADD CONSTRAINT location_organizers_fk FOREIGN KEY ( id_organizer )
        REFERENCES organizers ( id_organizer ) ON DELETE CASCADE;

ALTER TABLE meeting_rooms
    ADD CONSTRAINT meeting_room_room_fk FOREIGN KEY ( id_room )
        REFERENCES rooms ( id_room ) ON DELETE CASCADE;

--  ERROR: FK name length exceeds maximum allowed length(30) 
ALTER TABLE meeting_rrequests
    ADD CONSTRAINT meeting_rrequest_room_request_fk FOREIGN KEY ( id_room_request )
        REFERENCES room_requests ( id_room_request ) ON DELETE CASCADE;

ALTER TABLE notifications
    ADD CONSTRAINT notification_organizers_fk FOREIGN KEY ( id_organizer )
        REFERENCES organizers ( id_organizer ) ON DELETE CASCADE;

ALTER TABLE notifications
    ADD CONSTRAINT notification_room_request_fk FOREIGN KEY ( id_room_request )
        REFERENCES room_requests ( id_room_request ) ON DELETE CASCADE;

ALTER TABLE organizers
    ADD CONSTRAINT organizers_organizers_fk FOREIGN KEY ( id_organizer_substitute )
        REFERENCES organizers ( id_organizer ) ON DELETE CASCADE;

ALTER TABLE organizers
    ADD CONSTRAINT organizers_role_fk FOREIGN KEY ( id_role )
        REFERENCES roles ( id_role ) ON DELETE CASCADE;

ALTER TABLE presentation_rooms
    ADD CONSTRAINT presentation_room_room_fk FOREIGN KEY ( id_room )
        REFERENCES rooms ( id_room ) ON DELETE CASCADE;

--  ERROR: FK name length exceeds maximum allowed length(30) 
ALTER TABLE presentation_rrequests
    ADD CONSTRAINT presentation_rrequest_room_request_fk FOREIGN KEY ( id_room_request )
        REFERENCES room_requests ( id_room_request ) ON DELETE CASCADE;

ALTER TABLE reservations
    ADD CONSTRAINT reservations_organizers_fk FOREIGN KEY ( id_organizer )
        REFERENCES organizers ( id_organizer ) ON DELETE CASCADE;

ALTER TABLE reservations
    ADD CONSTRAINT reservations_room_fk FOREIGN KEY ( id_room )
        REFERENCES rooms ( id_room ) ON DELETE CASCADE;

ALTER TABLE reservations
    ADD CONSTRAINT reservations_room_request_fk FOREIGN KEY ( id_room_request )
        REFERENCES room_requests ( id_room_request )
            ON DELETE CASCADE;

ALTER TABLE rooms
    ADD CONSTRAINT room_location_fk FOREIGN KEY ( id_location )
        REFERENCES locations ( id_location ) ON DELETE CASCADE;

ALTER TABLE rooms
    ADD CONSTRAINT room_organizers_fk FOREIGN KEY ( id_organizer )
        REFERENCES organizers ( id_organizer ) ON DELETE CASCADE;

ALTER TABLE room_requests
    ADD CONSTRAINT room_request_location_fk FOREIGN KEY ( id_location )
        REFERENCES locations ( id_location ) ON DELETE CASCADE;

ALTER TABLE room_requests
    ADD CONSTRAINT room_request_organizers_fk FOREIGN KEY ( id_organizer )
        REFERENCES organizers ( id_organizer ) ON DELETE CASCADE;

CREATE OR REPLACE VIEW CITIES_V ( ID_CITY
   , name
   , ID_COUNTRY )
 AS SELECT
    ID_CITY
   , name
   , ID_COUNTRY
 FROM 
    CITIES 
;

CREATE OR REPLACE VIEW COUNTRIES_V ( ID_COUNTRY
   , name )
 AS SELECT
    ID_COUNTRY
   , name
 FROM 
    COUNTRIES 
;

CREATE OR REPLACE VIEW CREDENTIALS_V ( ID_CREDENTIAL
   , credential_type
   , data
   , created_at
   , ID_ORGANIZER )
 AS SELECT
    ID_CREDENTIAL
   , credential_type
   , data
   , created_at
   , ID_ORGANIZER
 FROM 
    CREDENTIALS 
;

CREATE OR REPLACE VIEW IMAGES_V ( ID_IMAGE
   , data
   , ID_ORGANIZER
   , ID_LOCATION
   , ID_ROOM )
 AS SELECT
    ID_IMAGE
   , data
   , ID_ORGANIZER
   , ID_LOCATION
   , ID_ROOM
 FROM 
    IMAGES 
;

CREATE OR REPLACE VIEW LOCATIONS_V ( ID_LOCATION
   , name
   , availability_start
   , availability_end
   , ID_CITY
   , ID_ORGANIZER )
 AS SELECT
    ID_LOCATION
   , name
   , availability_start
   , availability_end
   , ID_CITY
   , ID_ORGANIZER
 FROM 
    LOCATIONS 
;

CREATE OR REPLACE VIEW NOTIFICATIONS_V ( ID_NOTIFICATION
   , notification_type
   , delivered
   , ID_ORGANIZER
   , ID_ROOM_REQUEST )
 AS SELECT
    ID_NOTIFICATION
   , notification_type
   , delivered
   , ID_ORGANIZER
   , ID_ROOM_REQUEST
 FROM 
    NOTIFICATIONS 
;

CREATE OR REPLACE VIEW ORGANIZERS_V ( ID_ORGANIZER
   , name
   , email
   , ID_ROLE
   , ID_ORGANIZER_SUBSTITUTE )
 AS SELECT
    ID_ORGANIZER
   , name
   , email
   , ID_ROLE
   , ID_ORGANIZER_SUBSTITUTE
 FROM 
    ORGANIZERS 
;

CREATE OR REPLACE VIEW RESERVATIONS_V ( ID_RESERVATION
   , "start"
   , "end"
   , ID_ROOM
   , ID_ROOM_REQUEST
   , ID_ORGANIZER )
 AS SELECT
    ID_RESERVATION
   , "start"
   , "end"
   , ID_ROOM
   , ID_ROOM_REQUEST
   , ID_ORGANIZER
 FROM 
    RESERVATIONS 
;

CREATE OR REPLACE VIEW ROLES_V ( ID_ROLE
   , name )
 AS SELECT
    ID_ROLE
   , name
 FROM 
    ROLES 
;

CREATE OR REPLACE VIEW ROOM_REQUESTS_V ( id_organizer, id_location, id_room_request, min_capacity, reservation_start, reservation_end, reservation_length, type, vc_ready, podium_size ) AS
SELECT
    organizers.id_organizer,
    locations.id_location,
    room_requests.id_room_request,
    room_requests.min_capacity,
    room_requests.reservation_start,
    room_requests.reservation_end,
    room_requests.reservation_length,
    room_requests.type,
    meeting_rrequests.vc_ready,
    presentation_rrequests.podium_size
FROM
         room_requests
    INNER JOIN locations ON locations.id_location = room_requests.id_location
    INNER JOIN organizers ON organizers.id_organizer = room_requests.id_organizer,
    meeting_rrequests,
    presentation_rrequests 
;

CREATE OR REPLACE VIEW ROOMS_V (
    id_room,
    name,
    capacity,
    type,
    id_location,
    availability_start,
    availability_end,
    vc_ready,
    podium_size,
    id_organizer
) AS
SELECT
    r.id_room,
    r.name,
    r.capacity,
    r.type,
    l.id_location,
    l.availability_start,
    l.availability_end,
    mr.vc_ready,
    pr.podium_size,
    r.id_organizer
FROM
    rooms r
    INNER JOIN locations l ON l.id_location = r.id_location
    LEFT JOIN meeting_rooms mr ON mr.id_room = r.id_room
    LEFT JOIN presentation_rooms pr ON pr.id_room = r.id_room
;


CREATE OR REPLACE VIEW USERS_V ( ID_ORGANIZER, email, credential_type, data, name ) AS
SELECT
	o.ID_ORGANIZER,
	o.email,
	c.credential_type,
	c.data,
	r.name
FROM ORGANIZERS o 
     JOIN CREDENTIALS c ON o.ID_ORGANIZER = c.ID_ORGANIZER
	JOIN ROLES r ON o.ID_ROLE = r.ID_ROLE
ORDER BY c.created_at DESC 
;

CREATE OR REPLACE TRIGGER fkntm_organizers BEFORE
    UPDATE OF id_organizer_substitute ON organizers
BEGIN
    raise_application_error(-20225, 'Non Transferable FK constraint  on table ORGANIZERS is violated');
END;
/

CREATE OR REPLACE TRIGGER fkntm_reservations BEFORE
    UPDATE OF id_room_request ON reservations
BEGIN
    raise_application_error(-20225, 'Non Transferable FK constraint  on table RESERVATIONS is violated');
END;
/

CREATE OR REPLACE TRIGGER arc_fkarc_1_presentation_rooms BEFORE
    INSERT OR UPDATE OF id_room ON presentation_rooms
    FOR EACH ROW
DECLARE
    d VARCHAR2(17);
BEGIN
    SELECT
        a.type
    INTO d
    FROM
        rooms a
    WHERE
        a.id_room = :new.id_room;

    IF ( d IS NULL OR d <> 'PRESENTATION_ROOM' ) THEN
        raise_application_error(-20223, 'FK PRESENTATION_ROOM_ROOM_FK in Table PRESENTATION_ROOMS violates Arc constraint on Table ROOMS - discriminator column type doesn''t have value ''PRESENTATION_ROOM'''
        );
    END IF;

EXCEPTION
    WHEN no_data_found THEN
        NULL;
    WHEN OTHERS THEN
        RAISE;
END;
/

CREATE OR REPLACE TRIGGER arc_fkarc_1_meeting_rooms BEFORE
    INSERT OR UPDATE OF id_room ON meeting_rooms
    FOR EACH ROW
DECLARE
    d VARCHAR2(17);
BEGIN
    SELECT
        a.type
    INTO d
    FROM
        rooms a
    WHERE
        a.id_room = :new.id_room;

    IF ( d IS NULL OR d <> 'MEETING_ROOM' ) THEN
        raise_application_error(-20223, 'FK MEETING_ROOM_ROOM_FK in Table MEETING_ROOMS violates Arc constraint on Table ROOMS - discriminator column type doesn''t have value ''MEETING_ROOM'''
        );
    END IF;

EXCEPTION
    WHEN no_data_found THEN
        NULL;
    WHEN OTHERS THEN
        RAISE;
END;
/

CREATE OR REPLACE TRIGGER arc_fkarc_2_meeting_rrequests BEFORE
    INSERT OR UPDATE OF id_room_request ON meeting_rrequests
    FOR EACH ROW
DECLARE
    d VARCHAR2(21);
BEGIN
    SELECT
        a.type
    INTO d
    FROM
        room_requests a
    WHERE
        a.id_room_request = :new.id_room_request;

    IF ( d IS NULL OR d <> 'MEETING_RREQUEST' ) THEN
        raise_application_error(-20223, 'FK MEETING_RREQUEST_ROOM_REQUEST_FK in Table MEETING_RREQUESTS violates Arc constraint on Table ROOM_REQUESTS - discriminator column type doesn''t have value ''MEETING_RREQUEST'''
        );
    END IF;

EXCEPTION
    WHEN no_data_found THEN
        NULL;
    WHEN OTHERS THEN
        RAISE;
END;
/

CREATE OR REPLACE TRIGGER arc_fka_presentation_rrequests BEFORE
    INSERT OR UPDATE OF id_room_request ON presentation_rrequests
    FOR EACH ROW
DECLARE
    d VARCHAR2(21);
BEGIN
    SELECT
        a.type
    INTO d
    FROM
        room_requests a
    WHERE
        a.id_room_request = :new.id_room_request;

    IF ( d IS NULL OR d <> 'PRESENTATION_RREQUEST' ) THEN
        raise_application_error(-20223, 'FK PRESENTATION_RREQUEST_ROOM_REQUEST_FK in Table PRESENTATION_RREQUESTS violates Arc constraint on Table ROOM_REQUESTS - discriminator column type doesn''t have value ''PRESENTATION_RREQUEST'''
        );
    END IF;

EXCEPTION
    WHEN no_data_found THEN
        NULL;
    WHEN OTHERS THEN
        RAISE;
END;
/

CREATE SEQUENCE cities_id_city_seq START WITH 1 NOCACHE ORDER;

CREATE OR REPLACE TRIGGER cities_id_city_trg BEFORE
    INSERT ON cities
    FOR EACH ROW
    WHEN ( new.id_city IS NULL )
BEGIN
    :new.id_city := cities_id_city_seq.nextval;
END;
/

CREATE SEQUENCE countries_id_country_seq START WITH 1 NOCACHE ORDER;

CREATE OR REPLACE TRIGGER countries_id_country_trg BEFORE
    INSERT ON countries
    FOR EACH ROW
    WHEN ( new.id_country IS NULL )
BEGIN
    :new.id_country := countries_id_country_seq.nextval;
END;
/

CREATE SEQUENCE credentials_id_credential_seq START WITH 1 NOCACHE ORDER;

CREATE OR REPLACE TRIGGER credentials_id_credential_trg BEFORE
    INSERT ON credentials
    FOR EACH ROW
    WHEN ( new.id_credential IS NULL )
BEGIN
    :new.id_credential := credentials_id_credential_seq.nextval;
END;
/

CREATE SEQUENCE images_id_image_seq START WITH 1 NOCACHE ORDER;

CREATE OR REPLACE TRIGGER images_id_image_trg BEFORE
    INSERT ON images
    FOR EACH ROW
    WHEN ( new.id_image IS NULL )
BEGIN
    :new.id_image := images_id_image_seq.nextval;
END;
/

CREATE SEQUENCE locations_id_location_seq START WITH 1 NOCACHE ORDER;

CREATE OR REPLACE TRIGGER locations_id_location_trg BEFORE
    INSERT ON locations
    FOR EACH ROW
    WHEN ( new.id_location IS NULL )
BEGIN
    :new.id_location := locations_id_location_seq.nextval;
END;
/

CREATE SEQUENCE notifications_id_notification START WITH 1 NOCACHE ORDER;

CREATE OR REPLACE TRIGGER notifications_id_notification BEFORE
    INSERT ON notifications
    FOR EACH ROW
    WHEN ( new.id_notification IS NULL )
BEGIN
    :new.id_notification := notifications_id_notification.nextval;
END;
/

CREATE SEQUENCE organizers_id_organizer_seq START WITH 1 NOCACHE ORDER;

CREATE OR REPLACE TRIGGER organizers_id_organizer_trg BEFORE
    INSERT ON organizers
    FOR EACH ROW
    WHEN ( new.id_organizer IS NULL )
BEGIN
    :new.id_organizer := organizers_id_organizer_seq.nextval;
END;
/

CREATE SEQUENCE reservations_id_reservation START WITH 1 NOCACHE ORDER;

CREATE OR REPLACE TRIGGER reservations_id_reservation BEFORE
    INSERT ON reservations
    FOR EACH ROW
    WHEN ( new.id_reservation IS NULL )
BEGIN
    :new.id_reservation := reservations_id_reservation.nextval;
END;
/

CREATE SEQUENCE roles_id_role_seq START WITH 1 NOCACHE ORDER;

CREATE OR REPLACE TRIGGER roles_id_role_trg BEFORE
    INSERT ON roles
    FOR EACH ROW
    WHEN ( new.id_role IS NULL )
BEGIN
    :new.id_role := roles_id_role_seq.nextval;
END;
/

CREATE SEQUENCE room_requests_id_room_request START WITH 1 NOCACHE ORDER;

CREATE OR REPLACE TRIGGER room_requests_id_room_request BEFORE
    INSERT ON room_requests
    FOR EACH ROW
    WHEN ( new.id_room_request IS NULL )
BEGIN
    :new.id_room_request := room_requests_id_room_request.nextval;
END;
/

CREATE SEQUENCE rooms_id_room_seq START WITH 1 NOCACHE ORDER;

CREATE OR REPLACE TRIGGER rooms_id_room_trg BEFORE
    INSERT ON rooms
    FOR EACH ROW
    WHEN ( new.id_room IS NULL )
BEGIN
    :new.id_room := rooms_id_room_seq.nextval;
END;
/


-- Oracle SQL Developer Data Modeler Summary Report: 
-- 
-- CREATE TABLE                            15
-- CREATE INDEX                             6
-- ALTER TABLE                             43
-- CREATE VIEW                             12
-- ALTER VIEW                               0
-- CREATE PACKAGE                           0
-- CREATE PACKAGE BODY                      0
-- CREATE PROCEDURE                         0
-- CREATE FUNCTION                          0
-- CREATE TRIGGER                          17
-- ALTER TRIGGER                            0
-- CREATE COLLECTION TYPE                   0
-- CREATE STRUCTURED TYPE                   0
-- CREATE STRUCTURED TYPE BODY              0
-- CREATE CLUSTER                           0
-- CREATE CONTEXT                           0
-- CREATE DATABASE                          0
-- CREATE DIMENSION                         0
-- CREATE DIRECTORY                         0
-- CREATE DISK GROUP                        0
-- CREATE ROLE                              0
-- CREATE ROLLBACK SEGMENT                  0
-- CREATE SEQUENCE                         11
-- CREATE MATERIALIZED VIEW                 0
-- CREATE MATERIALIZED VIEW LOG             0
-- CREATE SYNONYM                           0
-- CREATE TABLESPACE                        0
-- CREATE USER                              0
-- 
-- DROP TABLESPACE                          0
-- DROP DATABASE                            0
-- 
-- REDACTION POLICY                         0
-- 
-- ORDS DROP SCHEMA                         0
-- ORDS ENABLE SCHEMA                       0
-- ORDS ENABLE OBJECT                       0
-- 
-- ERRORS                                   2
-- WARNINGS                                 0
