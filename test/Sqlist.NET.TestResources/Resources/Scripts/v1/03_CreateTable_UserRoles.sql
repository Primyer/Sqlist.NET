create table if not exists "UserRoles"
(
	"UserId" int,
	"RoleId" int,

	constraint "fkUserId" foreign key ("UserId")
		references "Users" ("Id")
		on delete cascade,

	constraint "fkRoleId" foreign key ("RoleId")
		references "Roles" ("Id")
		on delete cascade
);