CREATE OR REPLACE PACKAGE notifications_pkg AS
    
    PROCEDURE mark_notification_delivered(
        p_id_organizer IN NUMBER,
        p_notification_id IN NUMBER  -- JEN JEDNO ID
    );


        -- Získání počtu nepřečtených notifikací pro organizátora
    FUNCTION get_unread_notification_count(
        p_id_organizer IN NUMBER
    ) RETURN NUMBER;

END notifications_pkg;
/

CREATE OR REPLACE PACKAGE BODY notifications_pkg AS

    FUNCTION get_unread_notification_count(
        p_id_organizer IN NUMBER
    ) RETURN NUMBER
    IS
        v_count NUMBER;
    BEGIN
        SELECT COUNT(*)
        INTO v_count
        FROM notifications
        WHERE id_organizer = p_id_organizer
        AND delivered = 'N';
            
        RETURN v_count;
    EXCEPTION
        WHEN OTHERS THEN
            RETURN 0;
    END get_unread_notification_count;

        PROCEDURE mark_notification_delivered(
        p_id_organizer IN NUMBER,
        p_notification_id IN NUMBER  
    ) IS
    BEGIN
        UPDATE notifications
        SET delivered = 'Y'
        WHERE id_notification = p_notification_id
        AND id_organizer = p_id_organizer;
        
        COMMIT;
    END mark_notification_delivered;
    
END notifications_pkg;