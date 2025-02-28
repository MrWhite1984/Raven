MATCH(u:Users{UserId:$userId})-[r:Bookmark]->(p:Posts{PostId:$postId})
RETURN r