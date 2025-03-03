MATCH (c:Comments)-[:CommentTo]->(p:Posts{PostId:$postId})
WITH c.CommentId AS commentId, COLLECT(c) AS allComments
UNWIND allComments[0] AS uniqueComments
RETURN uniqueComments