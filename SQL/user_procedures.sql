-- Package pro správu uživatelù a impersonaci
CREATE OR REPLACE PACKAGE user_management_pkg AS
    -- Získání všech organizátorù kromì administrátorù (pro impersonaci)
    PROCEDURE get_non_admin_organizers(
        p_cursor OUT SYS_REFCURSOR
    );

END user_management_pkg;
/

CREATE OR REPLACE PACKAGE BODY user_management_pkg AS
    
    -- Získání všech organizátorù kromì administrátorù
    PROCEDURE get_non_admin_organizers(
        p_cursor OUT SYS_REFCURSOR
    ) IS
    BEGIN
        OPEN p_cursor FOR
            SELECT 
                o.id_organizer,
                o."name",
                o.email,
                o.id_role,
                r."name" as role_name
            FROM organizers o
            JOIN roles r ON o.id_role = r.id_role
            WHERE r."name" != 'Administrator'
            ORDER BY o."name";
    END get_non_admin_organizers;
    
END user_management_pkg;
/