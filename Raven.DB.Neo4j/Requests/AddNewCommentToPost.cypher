MATCH (p:Posts{PostId:$postId})
CREATE (c:Comments{CommnetId:$commentId}), (c)-[:CommentTo]->(p)