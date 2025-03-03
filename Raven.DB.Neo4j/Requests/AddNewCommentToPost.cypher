MATCH (p:Posts{PostId:$postId})
CREATE (c:Comments{CommentId:$commentId}), (c)-[:CommentTo]->(p)