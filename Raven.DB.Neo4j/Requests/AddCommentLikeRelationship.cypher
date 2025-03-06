MATCH (u:Users{UserId:$userId}), (c:Comments{CommentId:$commentId})
MERGE (u)-[:Like]->(c)