MATCH (u:Users{UserId:$userId}), (p:Posts), (u)-[r:Bookmark]->(p)
WHERE datetime(r.BookmarkDate) < datetime($cursor)
ORDER BY r.BookmarkDate DESC
WITH p.PostId AS postId, COLLECT(p) AS posts, COLLECT(r) AS rels
UNWIND posts[0] AS uniquePosts
LIMIT $pageSize
RETURN uniquePosts, rels