CREATE OR REPLACE PACKAGE rooms_pkg AS
    
    PROCEDURE persist_room (
        p_id_room IN NUMBER DEFAULT NULL,
        p_name IN VARCHAR2,
        p_capacity IN NUMBER,
        p_type IN VARCHAR2,
        p_id_location IN NUMBER,
        p_id_organizer IN NUMBER,
        p_vc_ready IN CHAR DEFAULT NULL, -- pro MEETING_ROOM
        p_podium_size IN NUMBER DEFAULT NULL -- pro PRESENTATION_ROOM
    );

    PROCEDURE delete_room (
        p_id_room IN NUMBER
    );

END rooms_pkg;
/

CREATE OR REPLACE PACKAGE BODY rooms_pkg AS 

    -- EDIT ROOM
    PROCEDURE persist_room (
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
        IF p_id_room IS NULL OR p_id_room = 0 THEN
            SELECT rooms_id_room_seq.NEXTVAL INTO v_new_id FROM dual;

            INSERT INTO rooms (id_room, "name", capacity, "type", id_location, id_organizer)
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
            SET "name" = p_name,
                capacity = p_capacity,
                "type" = p_type,
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
    END persist_room;


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

END rooms_pkg;