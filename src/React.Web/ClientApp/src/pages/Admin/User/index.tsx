import React, { useEffect } from "react";
import { useSelector } from "react-redux";
import MaterialTable, { Column } from "material-table";

import UserService from "../../../services/user-service";
import RoleService from "../../../services/role-service";
import { selectUser } from "../../../store/slices/auth-slice";
import { User } from "../../../models";

const Index = () => {
  const user = useSelector(selectUser);

  const [columns, setColumns] = React.useState<Column<User>[]>([]);

  useEffect(() => {
    const fetch = async () => {
      const role = await RoleService.findAllAsync();
      setColumns([
        { title: "Email", field: "email", editable: "never" },
        { title: "Role", field: "roleId", lookup: role.reduce((p, c) => ({ ...p, [c.id]: c.name }), {}) },
      ]);
    };
    fetch();
  }, []);

  return (
    <MaterialTable
      title="Users"
      columns={columns}
      data={async (query) => {
        const paged = await UserService.findAsync({ page: query.page + 1, pageSize: query.pageSize, queryString: query.search });
        return {
          data: paged.data,
          page: query.page,
          totalCount: paged.totalCount,
        };
      }}
      options={{
        actionsColumnIndex: -1,
        pageSize: 10,
        pageSizeOptions: [10, 20, 30],
        addRowPosition: "first"
      }}
      editable={{
        isEditHidden: (rowData) => rowData.id === user?.id,
        isDeleteHidden: (rowData) => rowData.id === user?.id,
        onRowUpdate: async (newData, oldData) => {
          if (oldData) {
            await UserService.updateAsync(oldData.id, newData.roleId);
          }
        },
        onRowDelete: async (oldData) => {
          await UserService.deleteAsync(oldData.id);
        },
      }}
    />
  );
};

export default Index;
