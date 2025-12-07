CREATE OR REPLACE PACKAGE admin_pkg AS

        -- V PACKAGE SPECIFICATION (za get_organizers_hierarchy)
    PROCEDURE list_database_objects(
        p_cursor OUT SYS_REFCURSOR
    );

    PROCEDURE get_audit_log(
        p_limit IN NUMBER DEFAULT 100,
        p_table_name IN VARCHAR2 DEFAULT NULL,
        p_cursor OUT SYS_REFCURSOR
    );
END admin_pkg;
/

CREATE OR REPLACE PACKAGE BODY admin_pkg AS 

    PROCEDURE get_audit_log(
    p_limit IN NUMBER DEFAULT 100,
    p_table_name IN VARCHAR2 DEFAULT NULL,
    p_cursor OUT SYS_REFCURSOR
    )
    IS
    BEGIN
        IF p_table_name IS NULL THEN
            OPEN p_cursor FOR
            SELECT 
                a.id_log,
                a.table_name,
                a.operation,
                a.record_id,
                a.id_organizer,
                NVL(o."name", 'System') as organizer_name,
                TO_CHAR(a.changed_at, 'DD.MM.YYYY HH24:MI:SS') as changed_at,
                a.old_values,
                a.new_values
            FROM audit_log a
            LEFT JOIN organizers o ON a.id_organizer = o.id_organizer
            ORDER BY a.changed_at DESC
            FETCH FIRST p_limit ROWS ONLY;
        ELSE
            OPEN p_cursor FOR
            SELECT 
                a.id_log,
                a.table_name,
                a.operation,
                a.record_id,
                a.id_organizer,
                NVL(o."name", 'System') as organizer_name,
                TO_CHAR(a.changed_at, 'DD.MM.YYYY HH24:MI:SS') as changed_at,
                a.old_values,
                a.new_values
            FROM audit_log a
            LEFT JOIN organizers o ON a.id_organizer = o.id_organizer
            WHERE a.table_name = p_table_name
            ORDER BY a.changed_at DESC
            FETCH FIRST p_limit ROWS ONLY;
        END IF;
    END get_audit_log;

    PROCEDURE list_database_objects(
        p_cursor OUT SYS_REFCURSOR
    )
    IS
    BEGIN
        OPEN p_cursor FOR
        SELECT 
            object_type,
            object_name,
            status,
            TO_CHAR(created, 'DD.MM.YYYY HH24:MI') as created_date,
            TO_CHAR(last_ddl_time, 'DD.MM.YYYY HH24:MI') as last_modified
        FROM user_objects
        WHERE object_type IN ('TABLE', 'VIEW', 'PROCEDURE', 'FUNCTION', 'TRIGGER', 'SEQUENCE', 'PACKAGE')
        ORDER BY object_type, object_name;
    END list_database_objects;

END admin_pkg;