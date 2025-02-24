MATCH (p:Posts) 
WHERE p.PostId = $postId
DETACH DELETE p