MATCH(u:Users{UserId:$userId})-[r:Like]->(p:Posts{PostId:$postId})
RETURN r