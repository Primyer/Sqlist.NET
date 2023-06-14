
-- Populate Users data
insert into "Users" values
	(1, 'User 1', 'user1@sqlist.net', '111111', null),
	(2, 'User 2', 'user2@sqlist.net', '222222', null),
	(3, 'User 3', 'user3@sqlist.net', '333333', null),
	(4, 'User 4', 'user4@sqlist.net', '444444', null);

insert into "Roles" values
	(1, 'Admin'),
	(2, 'User Manager'),
	(3, 'CEO'),
	(4, 'Staff Manager');

insert into "UserRoles" values
	(1, 1),
	(1, 2),
	(1, 4),
	(2, 1),
	(3, 1),
	(3, 3);