import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import categoryService from "../../services/category-service";

const Category = () => {
  const { id } = useParams<{ id: string }>();
  const [category, setCategory] = useState<{ id: number; name: string }>();

  useEffect(() => {
    const fetchCategory = async () => {
      var response = await categoryService.findAsync(+id);
      setCategory(response);
    };

    fetchCategory();
  }, [id, setCategory]);

  return (
    <div>
      {category && (
        <div>
          Category {category.id}, name {category.name},
        </div>
      )}
    </div>
  );
};

export default Category;
