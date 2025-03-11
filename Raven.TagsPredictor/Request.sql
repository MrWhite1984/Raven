SELECT  p."Body", array_agg(t."Name") AS "Tag" FROM "Posts" p
JOIN "PostTags" pt ON pt."PostId" = p."Id"
JOIN "Tags" t ON t."Id" = pt."TagId"
GROUP BY p."Id"
