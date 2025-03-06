MATCH (p:Posts{PostId:$postId})
MERGE (c:Comments{CommentId:$commentId})
CREATE (c)-[:CommentTo]->(p)