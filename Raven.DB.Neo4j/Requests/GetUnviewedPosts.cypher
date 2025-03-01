MATCH(u:Users{UserId:$userId}), (p:Posts)
WHERE NOT EXISTS ((u)-[:View]->(p))
WITH p.PostId AS postId, COLLECT(p) AS posts
UNWIND posts[0] AS uniquePosts
LIMIT $pageSize
RETURN uniquePosts