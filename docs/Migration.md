# Migration 파일이란?

EF Core에서 migration을 만들면 보통 3개 파일이 생긴다.

## `CreateUserTable.cs`

실제 DB 변경 내용을 담는 파일이다.

- `Up()` : 적용할 변경
- `Down()` : 되돌릴 변경

즉 테이블 생성, 컬럼 추가, 인덱스 생성 같은 실제 작업이 들어간다.

## `CreateUserTable.Designer.cs`

해당 migration이 만들어질 당시의 모델 상태를 EF Core가 기록해두는 파일이다.

보통 직접 수정하지 않는다.

## `AppDbContextModelSnapshot.cs`

현재 최신 모델 전체 상태를 저장하는 스냅샷 파일이다.

EF Core가 다음 migration을 만들 때 이전 상태와 비교하는 기준으로 사용한다.

즉 정리하면:
- `*.cs` : 실제 변경 내용
- `*.Designer.cs` : 해당 migration 시점의 내부 기록
- `AppDbContextModelSnapshot.cs` : 최신 전체 모델 스냅샷

## 자주 쓰는 명령어

Migration 생성:

```bash
dotnet ef migrations add CreateUserTable --project src/Riff.Api/Riff.Api.csproj --output-dir Data/Migrations
```

마지막 migration 제거:

```bash
dotnet ef migrations remove --project src/Riff.Api/Riff.Api.csproj
```

DB에 migration 적용:

```bash
dotnet ef database update --project src/Riff.Api/Riff.Api.csproj
```
