MATCH(u:Users{UserId:$userId})-[r:Bookmark]->(p:Posts{PostId:$postId})
RETURN r IS NOT NULL AS Exist