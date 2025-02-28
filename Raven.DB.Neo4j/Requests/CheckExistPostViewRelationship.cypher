MATCH(u:Users{UserId:$userId})-[r:View]->(p:Posts{PostId:$postId})
RETURN r IS NOT NULL AS Exist