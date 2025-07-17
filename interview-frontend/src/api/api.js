const api_url = "http://localhost:10010";

export const startPlan = async () => {
    const url = `${api_url}/Plan`;
    try {
        const response = await fetch(url, {
            method: "POST",
            headers: {
                Accept: "application/json",
                "Content-Type": "application/json",
            },
            body: JSON.stringify({}),
        });

        if (!response.ok) {
            console.error("Failed to create plan - API Response:", response);
            alert("Failed to start plan. Please try again.");
            return null;
        }

        return await response.json();
    } catch (e) {
        console.error("Internal Sever error during startPlan:", e);
        alert("An internal sever error occurred. Please check your connection.");
        return null;
    }
};

export const addProcedureToPlan = async (planId, procedureId) => {
    const url = `${api_url}/Plan/AddProcedureToPlan`;
    var command = { planId: planId, procedureId: procedureId };
    try {
        const response = await fetch(url, {
            method: "POST",
            headers: {
                Accept: "application/json",
                "Content-Type": "application/json",
            },
            body: JSON.stringify(command),
        });

        if (!response.ok) {
            console.error("Failed to add procedure to plan - API Response:", response);
            alert("Failed to add procedure. Please try again.");
            return false;
        }

        return true;
    } catch (e) {
        console.error("Internal Sever error during addProcedureToPlan:", e);
        alert("An internal sever error occurred. Please check your connection.");
        return false;
    }
};

export const getProcedures = async () => {
    const url = `${api_url}/Procedures`;
    try {
        const response = await fetch(url, {
            method: "GET",
        });

        if (!response.ok) {
            console.error("Failed to get procedures - API Response:", response);
            alert("Failed to load procedures. Please try again.");
            return [];
        }

        return await response.json();
    } catch (e) {
        console.error("Internal Sever error during getProcedures:", e);
        alert("An internal sever error occurred. Please check your connection.");
        return [];
    }
};

export const getPlanProcedures = async (planId) => {
    const url = `${api_url}/PlanProcedure?$filter=planId eq ${planId}&$expand=procedure`;
    try {
        const response = await fetch(url, {
            method: "GET",
        });

        if (!response.ok) {
            console.error("Failed to get plan procedures - API Response:", response);
            alert("Failed to load plan procedures. Please try again.");
            return [];
        }

        return await response.json();
    } catch (e) {
        console.error("Internal Sever error during getPlanProcedures:", e);
        alert("An internal sever error occurred. Please check your connection.");
        return [];
    }
};

export const getUsers = async () => {
    const url = `${api_url}/Users`;
    try {
        const response = await fetch(url, {
            method: "GET",
        });

        if (!response.ok) {
            console.error("Failed to get users - API Response:", response);
            alert("Failed to load users. Please try again.");
            return [];
        }

        return await response.json();
    } catch (e) {
        console.error("Internal Sever error during getUsers:", e);
        alert("An internal sever error occurred. Please check your connection.");
        return [];
    }
};

export const getPlanProcedureUsers = async (planProcedureId) => {
    const url = `${api_url}/PlanProcedure/Users?PlanProcedureId=${planProcedureId}`;
    try {
        const response = await fetch(url, {
            method: "GET",
        });

        if (!response.ok) {
            console.error("Failed to get plan procedure users - API Response:", response);
            alert("Failed to load assigned users. Please try again.");
            return [];
        }

        return await response.json();
    } catch (e) {
        console.error("Internal Sever error during getPlanProcedureUsers:", e);
        alert("An internal sever error occurred. Please check your connection.");
        return [];
    }
};

export const addUserToPlanProcedure = async (planProcedureId, userId) => {
    const url = `${api_url}/PlanProcedure/AddUser`;
    var command = { planProcedureId: planProcedureId, userId: userId };
    try {
        const response = await fetch(url, {
            method: "POST",
            headers: {
                Accept: "application/json",
                "Content-Type": "application/json",
            },
            body: JSON.stringify(command),
        });

        if (!response.ok) {
            console.error("Failed to add user to plan procedure - API Response:", response);
            alert("Failed to add user. Please try again.");
            return false;
        }

        return true;
    } catch (e) {
        console.error("Internal Sever error during addUserToPlanProcedure:", e);
        alert("An internal sever error occurred. Please check your connection.");
        return false;
    }
};

export const removeUserFromPlanProcedure = async (planProcedureId, userId) => {
    const url = `${api_url}/PlanProcedure/RemoveUser`;
    var command = { planProcedureId: planProcedureId, userId: `${userId}` }; // Ensure userId is string for API
    try {
        const response = await fetch(url, {
            method: "DELETE",
            headers: {
                Accept: "application/json",
                "Content-Type": "application/json",
            },
            body: JSON.stringify(command),
        });

        if (!response.ok) {
            console.error("Failed to remove user from plan procedure - API Response:", response);
            alert("Failed to remove user. Please try again.");
            return false;
        }

        return true;
    } catch (e) {
        console.error("Internal Sever error during removeUserFromPlanProcedure:", e);
        alert("An internal sever error occurred. Please check your connection.");
        return false;
    }
};

export const removeAllUserFromPlanProcedure = async (planProcedureId) => {
    const url = `${api_url}/PlanProcedure/RemoveUser`;
    var command = { planProcedureId: planProcedureId, userId: '*' };
    try {
        const response = await fetch(url, {
            method: "DELETE",
            headers: {
                Accept: "application/json",
                "Content-Type": "application/json",
            },
            body: JSON.stringify(command),
        });

        if (!response.ok) {
            console.error("Failed to remove all users from plan procedure - API Response:", response);
            alert("Failed to remove all users. Please try again.");
            return false;
        }

        return true;
    } catch (e) {
        console.error("Internal Sever error during removeAllUserFromPlanProcedure:", e);
        alert("An internal sever error occurred. Please check your connection.");
        return false;
    }
};