MATCH (c1:Comments{CommentId:$commentToId})
CREATE (c2:Comments{CommentId:$commentId}), (c2)-[:CommentTo]->(c1)