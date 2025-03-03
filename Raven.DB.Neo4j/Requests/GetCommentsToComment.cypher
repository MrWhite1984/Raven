MATCH (c1:Comments)-[:CommentTo]->(c2:Comments{CommentId:$commentToId})
WITH c1.CommentId AS commentId, COLLECT(c1) AS allComments
UNWIND allComments[0] AS uniqueComments
RETURN uniqueComments