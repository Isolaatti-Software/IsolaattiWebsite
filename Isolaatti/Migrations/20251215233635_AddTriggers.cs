using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Isolaatti.Migrations
{
    /// <inheritdoc />
    public partial class AddTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION set_ranking_by_posting()
    RETURNS TRIGGER
    LANGUAGE plpgsql
AS
$$
BEGIN
    UPDATE ""SquadUsers"" AS su
    SET ""Ranking""                 = ""Ranking"" + 1.0,
        ""LastInteractionDateTime"" = NEW.""Date""
    WHERE su.""UserId"" = NEW.""UserId""
      AND su.""SquadId"" = NEW.""SquadId"";
    RETURN NULL;
END;
$$;

CREATE TRIGGER squadPostInserted
    AFTER INSERT
    ON ""SimpleTextPosts""
    FOR EACH ROW
EXECUTE FUNCTION set_ranking_by_posting();

CREATE OR REPLACE FUNCTION set_ranking_by_commenting()
    RETURNS TRIGGER
    LANGUAGE plpgsql
AS
$$
BEGIN
    UPDATE ""SquadUsers"" AS su
    SET ""Ranking"" = ""Ranking"" + 0.5,
        ""LastInteractionDateTime"" = NEW.""Date""
    WHERE su.""UserId"" = NEW.""UserId""
      AND su.""SquadId"" = NEW.""SquadId"";
    RETURN NULL;
END;
$$;


CREATE TRIGGER squadCommentInserted
    AFTER INSERT
    ON ""Comments""
    FOR EACH ROW
EXECUTE FUNCTION set_ranking_by_commenting();
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TRIGGER squadCommentInserted ON ""Comments"";
DROP FUNCTION set_ranking_by_commenting;
DROP TRIGGER squadPostInserted ON ""SimpleTextPosts"";
DROP FUNCTION set_ranking_by_posting;
");
        }
    }
}
