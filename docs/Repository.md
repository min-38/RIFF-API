# Repository란?

`Repository`는 데이터 조회/저장을 담당하는 계층이다.

쉽게 말하면 서비스가 DB를 직접 다루지 않고, 중간에서 조회와 저장을 맡아주는 역할이다.

예를 들어 회원가입/로그인에서는 이런 기능이 들어간다.
- 이메일로 사용자 조회
- 유저네임 중복 확인
- 사용자 저장

즉:
- `SignupService`, `LoginService`는 흐름을 처리하고
- `UserRepository`는 사용자 조회/저장을 처리한다.

예시 메서드:
- `GetByEmailAsync`
- `ExistsByUsernameAsync`
- `AddAsync`
