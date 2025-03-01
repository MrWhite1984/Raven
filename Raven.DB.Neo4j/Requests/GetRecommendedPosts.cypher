MATCH(u:Users{UserId:$userId}), (p:Posts), (u)-[r:Recomended]->(p)
ORDER BY r.Date
WITH p.PostId AS postId, COLLECT(p) AS posts
UNWIND posts[0] AS uniquePosts
LIMIT $pageSize
RETURN uniquePosts