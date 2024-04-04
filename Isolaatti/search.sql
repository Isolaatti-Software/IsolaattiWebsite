CREATE INDEX idx_users_full_text_username
    ON "Users"
    USING GIN(to_tsvector('english', "Users"."UniqueUsername"));

CREATE INDEX idx_users_full_text_name
    ON "Users"
        USING GIN(to_tsvector('english', "Users"."Name" ));

CREATE INDEX idx_users_full_text_email
    ON "Users"
        USING GIN(to_tsvector('english', "Users"."Email"));

CREATE INDEX idx_posts_full_text
    ON "SimpleTextPosts"
    USING GIN(to_tsvector('english', "SimpleTextPosts"."TextContent"));

CREATE INDEX idx_hashtags_full_text
    ON "Hashtags"
    USING GIN(to_tsvector('english', "Hashtags"."Text"));

CREATE VIEW View_SearchItems AS
    SELECT 
        text 'users' AS origin_table, 
        "Id"::text AS id, 
        "UniqueUsername" AS title,
        "Name" AS description,
        to_tsvector('english', "Users"."UniqueUsername")
            || to_tsvector('english',  "Users"."Name")
            || to_tsvector('english', "Users"."Email") AS searchable_element
    FROM "Users"
UNION ALL
    SELECT
        text 'posts' AS origin_table,
        "Id"::text AS id,
        substr(replace("TextContent", E'\n', ' '), 0, 20) as title,
        "Date"::text as description,
        to_tsvector("SimpleTextPosts"."TextContent") AS searchable_element
    FROM "SimpleTextPosts"
UNION ALL
    SELECT
        text 'hashtags' AS origin_table,
        "Text"::text AS id,
        '#' || "Text" as title,
        '' as description,
        to_tsvector("Hashtags"."Text") AS searchable_element
    FROM "Hashtags";

SELECT origin_table, id, description, title, ts_rank(searchable_element, to_tsquery('english', 'taylorswift')) AS rank FROM View_SearchItems WHERE to_tsquery('english', 'taylorswift') @@ View_SearchItems.searchable_element ORDER BY rank DESC;