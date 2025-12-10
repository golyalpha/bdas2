CREATE OR REPLACE PACKAGE admin_pkg AS

    PROCEDURE list_database_objects(
        p_cursor OUT SYS_REFCURSOR
    );

    TYPE t_audit_record IS RECORD (
        id_log NUMBER,
        table_name VARCHAR2(30),
        operation VARCHAR2(10),
        record_id NUMBER,
        organizer_name VARCHAR2(64),
        changed_at TIMESTAMP,
        old_values VARCHAR2(1000),
        new_values VARCHAR2(1000),
        total_count NUMBER
    );

    TYPE t_audit_records IS TABLE OF t_audit_record;

    FUNCTION get_audit_page(
        p_page_number IN NUMBER DEFAULT 1,
        p_page_size IN NUMBER DEFAULT 50,
        p_table_name IN VARCHAR2 DEFAULT NULL,
        p_operation IN VARCHAR2 DEFAULT NULL,
        p_date_from IN DATE DEFAULT NULL,
        p_date_to IN DATE DEFAULT NULL
    ) RETURN t_audit_records PIPELINED;

    FUNCTION get_total_count(
        p_table_name IN VARCHAR2 DEFAULT NULL,
        p_operation IN VARCHAR2 DEFAULT NULL,
        p_date_from IN DATE DEFAULT NULL,
        p_date_to IN DATE DEFAULT NULL
    ) RETURN NUMBER;
    
END admin_pkg;
/

CREATE OR REPLACE PACKAGE BODY admin_pkg AS 

    FUNCTION get_total_count(
        p_table_name IN VARCHAR2 DEFAULT NULL,
        p_operation IN VARCHAR2 DEFAULT NULL,
        p_date_from IN DATE DEFAULT NULL,
        p_date_to IN DATE DEFAULT NULL
    ) RETURN NUMBER IS
        v_count NUMBER;
    BEGIN
        SELECT COUNT(*)
        INTO v_count
        FROM audit_log a
        WHERE (p_table_name IS NULL OR a.table_name = p_table_name)
          AND (p_operation IS NULL OR a.operation = p_operation)
          AND (p_date_from IS NULL OR a.changed_at >= p_date_from)
          AND (p_date_to IS NULL OR a.changed_at <= p_date_to);
        
        RETURN v_count;
    END get_total_count;

    FUNCTION get_audit_page(
        p_page_number IN NUMBER DEFAULT 1,
        p_page_size IN NUMBER DEFAULT 50,
        p_table_name IN VARCHAR2 DEFAULT NULL,
        p_operation IN VARCHAR2 DEFAULT NULL,
        p_date_from IN DATE DEFAULT NULL,
        p_date_to IN DATE DEFAULT NULL
    ) RETURN t_audit_records PIPELINED IS
        v_offset NUMBER;
        v_total_count NUMBER;
        v_record t_audit_record;
    BEGIN
        v_offset := (p_page_number - 1) * p_page_size;
        
        v_total_count := get_total_count(
            p_table_name, 
            p_operation, 
            p_date_from, 
            p_date_to
        );
        
        FOR rec IN (
            SELECT * FROM (
                SELECT 
                    a.id_log,
                    a.table_name,
                    a.operation,
                    a.record_id,
                    NVL(o."name", 'System') as organizer_name,
                    a.changed_at,
                    a.old_values,
                    a.new_values,
                    v_total_count as total_count,
                    ROW_NUMBER() OVER (ORDER BY a.changed_at DESC, a.id_log DESC) as rn
                FROM audit_log a
                LEFT JOIN organizers o ON a.id_organizer = o.id_organizer
                WHERE (p_table_name IS NULL OR a.table_name = p_table_name)
                  AND (p_operation IS NULL OR a.operation = p_operation)
                  AND (p_date_from IS NULL OR a.changed_at >= p_date_from)
                  AND (p_date_to IS NULL OR a.changed_at <= p_date_to)
            )
            WHERE rn > v_offset AND rn <= (v_offset + p_page_size)
        ) LOOP
            v_record.id_log := rec.id_log;
            v_record.table_name := rec.table_name;
            v_record.operation := rec.operation;
            v_record.record_id := rec.record_id;
            v_record.organizer_name := rec.organizer_name;
            v_record.changed_at := rec.changed_at;
            v_record.old_values := rec.old_values;
            v_record.new_values := rec.new_values;
            v_record.total_count := rec.total_count;
            
            PIPE ROW(v_record);
        END LOOP;
        
        RETURN;
    END get_audit_page;

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