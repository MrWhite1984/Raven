MATCH (u1:Users)-[:Like]->(p:Posts)<-[:Like]-(u2:Users)
WHERE u1.UserId = $userId AND u1<>u2
WITH u1, u2, COUNT(p) AS posts_user_2
ORDER BY posts_user_2 DESC
LIMIT 5
MATCH (u2)-[:Like]->(rec_posts:Posts)
WHERE NOT EXISTS ((u1)-[:View]->(rec_posts))
  AND NOT EXISTS ((u1)-[:Recomended]->(rec_posts))
LIMIT 20
CREATE (u1)-[:Recomended{Date:datetime()}]->(rec_posts)