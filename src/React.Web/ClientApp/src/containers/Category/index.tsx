import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import categoryService, { CategoryResponse } from "../../services/category-service";

const Category = () => {
  const { id } = useParams<{ id: string }>();
  const [category, setCategory] = useState<CategoryResponse>();

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
          Category {category.id}, name {category.name}
          {category.products.map((product) => (
            <Product key={product.id} product={product} />
          ))}
        </div>
      )}
    </div>
  );
};

const Product = ({ product }) => {
  return (
    <div>
      <div>{product.name}</div>
      <div>
        <img alt="" src={product.imageFileName} />
      </div>
    </div>
  );
};

export default Category;
