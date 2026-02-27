# EF Core란?

`EF Core(Entity Framework Core)`는 .NET에서 데이터베이스를 객체처럼 다루게 해주는 ORM이다.

즉 SQL을 직접 많이 작성하지 않고도:
- 조회
- 추가
- 수정
- 삭제

를 C# 코드로 처리할 수 있다.

이 프로젝트에서는 PostgreSQL과 연결해서 사용할 예정이며, `AppDbContext`가 EF Core의 진입점 역할을 한다.
