import { useEffect, useState } from "react";
import ReactSelect from "react-select";
import {
  getPlanProcedureUsers,
  addUserToPlanProcedure,
  removeUserFromPlanProcedure,
  removeAllUserFromPlanProcedure,
} from "../../../api/api.js";

const PlanProcedureItem = ({ planProcedure, users }) => {
  const [selectedUsers, setSelectedUsers] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const procedure = planProcedure.procedure;
  const planProcedureId = planProcedure.planProcedureId;

  // Load existing users for this procedure when component mounts
  useEffect(() => {
    const loadUsers = async () => {
      try {
        setIsLoading(true);
        const existingUsers = await getPlanProcedureUsers(planProcedureId);
        const mapped = existingUsers.map((u) => ({
          value: u.userId,
          label: u.name,
        }));
        setSelectedUsers(mapped);
      } catch (err) {
        console.error("Failed to load users:", err);
      } finally {
        setIsLoading(false);
      }
    };

    loadUsers();
  }, [planProcedureId]);

  const handleAssignUserToProcedure = async (newSelected) => {
    const previousIds = selectedUsers.map((u) => u.value);
    const newIds = newSelected.map((u) => u.value);

    const added = newIds.filter((id) => !previousIds.includes(id));
    const removed = previousIds.filter((id) => !newIds.includes(id));

    try {
      setIsLoading(true);

      if (newSelected.length === 0) {
        await removeAllUserFromPlanProcedure(planProcedureId);
      } else {
        // Add newly selected users
        for (const id of added) {
          await addUserToPlanProcedure(planProcedureId, id);
        }

        // Remove unselected users
        for (const id of removed) {
          await removeUserFromPlanProcedure(planProcedureId, id);
        }
      }

      setSelectedUsers(newSelected);
    } catch (err) {
      console.error("Error updating users:", err);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="py-2">
      <div>{procedure.procedureTitle}</div>

      <ReactSelect
        className="mt-2"
        placeholder="Select User(s) to Assign"
        isMulti
        options={users}
        value={selectedUsers}
        onChange={handleAssignUserToProcedure}
        isDisabled={isLoading}
      />
    </div>
  );
};

export default PlanProcedureItem;
