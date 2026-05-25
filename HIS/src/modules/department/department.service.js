const Department = require('./models/department.model');

const getAllDepartments = async () => {
  return await Department.find();
};

const getDepartmentById = async (id) => {
  return await Department.findById(id);
};

const createDepartment = async (departmentData) => {
  const department = new Department(departmentData);
  return await department.save();
};

const updateDepartment = async (id, departmentData) => {
  return await Department.findByIdAndUpdate(id, departmentData, { new: true });
};

const deleteDepartment = async (id) => {
  return await Department.findByIdAndDelete(id);
};

module.exports = {
  getAllDepartments,
  getDepartmentById,
  createDepartment,
  updateDepartment,
  deleteDepartment,
};