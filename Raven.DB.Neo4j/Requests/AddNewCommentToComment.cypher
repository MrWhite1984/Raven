MATCH (c1:Comments{CommentId:$commentToId})
MERGE (c2:Comments{CommentId:$commentId})
CREATE (c2)-[:CommentTo]->(c1)