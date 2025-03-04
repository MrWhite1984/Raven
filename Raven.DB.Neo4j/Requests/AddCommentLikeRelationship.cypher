MATCH (u:Users{UserId:$userId}), (c:Comments{CommentId:$commentId})
CREATE (u)-[:Like]->(c)